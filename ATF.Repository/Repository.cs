namespace ATF.Repository
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using ATF.Repository.Attributes;
	using ATF.Repository.Builder;
	using ATF.Repository.Mapping;
	using Terrasoft.Core;
	using Terrasoft.Core.DB;
	using Terrasoft.Core.Entities;
	using Terrasoft.Core.Factories;

	[DefaultBinding(typeof(IRepository))]
	public class Repository : IRepository
	{

		#region Fields: Private

		private Dictionary<Guid, BaseModel> _items;
		private Dictionary<Guid, Entity> _entities;
		private ModelMapper _modelMapper;
		private ProxyClassBuilder _proxyClassBuilder;
		private List<BaseModel> _itemsToDelete;

		#endregion

		#region Properties: Protected

		public bool DataStoreEnabled {
			get {
				return UserConnection != null;
			}
		}

		#endregion

		#region Properties: Public

		public UserConnection UserConnection { private get; set; }

		#endregion

		#region Constructors: Public

		public Repository() {
			_items = new Dictionary<Guid, BaseModel>();
			_entities = new Dictionary<Guid, Entity>();
			_modelMapper = new ModelMapper();
			_proxyClassBuilder = new ProxyClassBuilder(this);
			_itemsToDelete = new List<BaseModel>();
		}

		#endregion

		#region Methods: Private

		private string GetEntitySchemaName(Type type) {
			string name = string.Empty;
			if (Attribute.IsDefined(type, typeof(SchemaAttribute)) &&
				(type.GetCustomAttribute(typeof(SchemaAttribute)) as SchemaAttribute) != null) {
				SchemaAttribute attribute = type.GetCustomAttribute(typeof(SchemaAttribute)) as SchemaAttribute;
				name = attribute.Name;
			}
			return name;
		}

		private List<T> LoadModelsByFilter<T>(string propertyName, Guid filterValue) where T : BaseModel, new() {
			var response = new List<T>();
			var columnName = GetColumnNameByPropertyName(typeof(T), propertyName);
			var recordIds = GetPartialLoadRecordIds(typeof(T), columnName, filterValue);
			foreach (var recordId in recordIds) {
				var item = LoadModelById<T>(recordId);
				response.Add(item);
			}
			return response;
		}

		private string GetColumnNameByPropertyName(Type type, string propertyName) {
			var mapper = _modelMapper.GetParameters(type).Where(x => x.Name == propertyName).FirstOrDefault();
			var entitySchema = GetEntitySchema(type);
			var schemaColumn = entitySchema?.Columns.GetByName(mapper.EntitySchemaColumnName);
			return schemaColumn != null
				? schemaColumn.ColumnValueName
				: string.Empty;
		}

		private MethodInfo GetLoadModelMethod(Type genericType) {
			return GetGenericMethod(genericType, "LoadModelById");
		}

		private MethodInfo GetLoadModelListMethod(Type genericType) {
			return GetGenericMethod(genericType, "LoadModelsByFilter");
		}

		private MethodInfo GetGenericMethod(Type genericType, string methodName) {
			MethodInfo response = this.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
			return response != null
				? response.MakeGenericMethod(genericType)
				: null;
		}

		private T LoadModelById<T>(Guid id) where T : BaseModel, new() {
			if (id == Guid.Empty) {
				return default(T);
			}
			if (_items.ContainsKey(id)) {
				return (T)_items[id];
			}
			var item = CreateModelById<T>(id);
			
			return item;
		}

		private T CreateModelById<T>(Guid id) where T : BaseModel, new() {
			var model = CreateModel<T>();
			FillPropertyValues(model, id);
			_items.Add(model.Id, model);
			FillReferenceValues(model);
			FillDetailValues(model);
			return model;
		}

		private T CreateModel<T>() where T: BaseModel, new() {
			var model = _proxyClassBuilder.Build<T>();
			model.UserConnection = UserConnection;
			return model;
		}

		private void FillPropertyValues<T>(T model, Guid id) where T : BaseModel {
			var valuesFromEntity = GetValuesFromEntity<T>(id);
			foreach (var item in valuesFromEntity) {
				PropertyInfo propertyInfo = model.GetType().GetProperty(item.Key);
				if (item.Value != null || Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null) {
					propertyInfo.SetValue(model, Convert.ChangeType(item.Value, propertyInfo.PropertyType), null);
				}
			}
			if (model.Id == Guid.Empty) {
				model.Id = Guid.NewGuid();
			}
		}

		private Guid GetReferenceValue<T>(T model, string propertyName) {
			var property = typeof(T).GetProperty(propertyName);
			object value = null;
			if (property != null && property.PropertyType == typeof(Guid)) {
				value = property.GetValue(model);
			}
			return ConvertToGuid(value);
		}

		private Guid ConvertToGuid(object value) {
			return value != null && value.GetType() == typeof(Guid)
				? (Guid)value
				: Guid.Empty;
		}

		private void FillReferenceValues<T>(T model) where T : BaseModel {
			var references = _modelMapper.GetReferences(typeof(T));
			foreach (var reference in references.Where(x => !x.IsLazyLoad)) {
				FillReferenceValue(model, reference);
			}
		}

		internal void FillReferenceValue<T>(T model, ModelReference reference) {
			if (!DataStoreEnabled) {
				return;
			}
			var property = typeof(T).GetProperty(reference.Name);
			var referenceId = GetReferenceValue<T>(model, reference.ValuePropertyName);
			if (referenceId != Guid.Empty && property != null) {
				var method = GetLoadModelMethod(reference.Type);
				property.SetValue(model, method.Invoke(this, new object[] { referenceId }));
			}
		}

		private void FillDetailValues<T>(T model) where T : BaseModel {
			var details = _modelMapper.GetDetails(typeof(T));
			foreach (var detail in details.Where(x => !x.IsLazyLoad)) {
				FillDetailValue(model, detail);
			}
		}

		internal void FillDetailValue<T>(T model, ModelDetail detail) {
			var valueProperty = typeof(T).GetProperty(detail.Name);
			var masterFilterPropertyName = !string.IsNullOrEmpty(detail.MasterFilterPropertyName)
				? detail.MasterFilterPropertyName
				: "Id";
			var masterId = GetReferenceValue<T>(model, masterFilterPropertyName);
			if (valueProperty != null) {
				var method = GetLoadModelListMethod(detail.Type);
				valueProperty.SetValue(model, method.Invoke(this, new object[] { detail.DetailFilterPropertyName, masterId }));
			}
		}

		private IDictionary<string, object> GetValuesFromEntity<T>(Guid id) {
			var entity = GetEntity(typeof(T), id);
			return entity != null
				? GetValuesFromEntity<T>(entity)
				: new Dictionary<string, object>();
		}

		private IDictionary<string, object> GetValuesFromEntity<T>(Entity entity) {
			var parameters = _modelMapper.GetParameters(typeof(T));
			return entity != null
				? GetValuesFromEntity(entity, parameters)
				: new Dictionary<string, object>();
		}

		private Dictionary<string, object> GetValuesFromEntity(Entity entity, List<ModelParameter> parameters) {
			var response = new Dictionary<string, object>();
			foreach (var item in parameters) {
				var schemaColumn = entity.Schema.Columns.GetByName(item.EntitySchemaColumnName);
				response.Add(item.Name, entity.GetColumnValue(schemaColumn.ColumnValueName));
			}
			return response;
		}

		private Entity GetEntity(Type type, Guid id) {
			if (_entities.ContainsKey(id)) {
				return _entities[id];
			}
			var entity = id != Guid.Empty
				? LoadExistedEntity(type, id)
				: CreateNewEntity(type);
			if (entity != null) {
				_entities.Add(entity.PrimaryColumnValue, entity);
			}
			return entity;
		}

		private Entity LoadExistedEntity(Type type, Guid id) {
			var schema = GetEntitySchema(type);
			var entity = schema?.CreateEntity(UserConnection);
			return entity != null && entity.FetchFromDB(id)
				? entity
				: null;
		}

		private Entity CreateNewEntity(Type type) {
			var schema = GetEntitySchema(type);
			Entity entity = schema?.CreateEntity(UserConnection);
			if (schema != null) {
				entity.SetDefColumnValues();
			}
			return entity;
		}

		private EntitySchema GetEntitySchema(Type type) {
			var name = GetEntitySchemaName(type);
			return DataStoreEnabled
				? UserConnection.EntitySchemaManager.GetInstanceByName(name)
				: null;
		}

		private List<Guid> GetPartialLoadRecordIds(Type type, string columnFilterName, Guid filterValue) {
			List<Guid> response = new List<Guid>();
			string entitySchemaName = GetEntitySchemaName(type);
			if (DataStoreEnabled && filterValue != Guid.Empty && entitySchemaName != string.Empty) {
				var select = new Select(UserConnection)
					.Column("Id")
					.From(entitySchemaName)
					.Where(columnFilterName).IsEqual(new QueryParameter(filterValue)) as Select;
				select.ExecuteReader(dataReader => {
					if (!dataReader.IsDBNull(dataReader.GetOrdinal("Id"))) {
						response.Add(dataReader.GetGuid(dataReader.GetOrdinal("Id")));
					}
				});
			}
			return response;
		}

		private void SaveModel<T>(T model) where T : BaseModel {
			var parameters = _modelMapper.GetParameters(model.GetType());
			var entity = GetEntity(model.GetType(), model.Id);
			if (entity == null) {
				return;
			}
			bool isChanged = false;
			foreach (var parameterInfo in parameters) {
				isChanged = TrySetEntityValue(model, entity, parameterInfo) || isChanged;
			}
			if (isChanged) {
				entity.Save(false);
			}
		}

		private bool TrySetEntityValue<T>(T source, Entity target, ModelParameter parameter) where T : BaseModel {
			var oldValue = GetEntityValue(target, parameter.EntitySchemaColumnName);
			var newValue = GetModelValue(source, parameter.Name);
			if (!Equals(oldValue, newValue)) {
				var columnName = GetColumnNameByPropertyName(source.GetType(), parameter.Name);
				target.SetColumnValue(columnName, newValue);
				return true;
			}
			return false;
		}


		private object GetEntityValue(Entity entity, string columnName) {
			var schemaColumn = entity.Schema.Columns.GetByName(columnName);
			return entity.GetColumnValue(schemaColumn);
		}

		private object GetModelValue<T>(T model, string propertyName) where T : BaseModel {
			var propertyInfo = model.GetType().GetProperty(propertyName);
			return PrepareModelValueForSave(propertyInfo.GetValue(model), propertyInfo.PropertyType);
		}

		private object PrepareModelValueForSave(object value, Type type) {
			if ((type == typeof(DateTime) && (DateTime)value == DateTime.MinValue) ||
				(type == typeof(Guid) && (Guid)value == Guid.Empty)) {
				return null;
			}
			return value;
		}

		private void DeleteModel<T>(T model) where T : BaseModel {
			var entity = GetEntity(model.GetType(), model.Id);
			if (entity == null) {
				return;
			}
			if (entity.Delete()) {
				_items.Remove(model.Id);
				_itemsToDelete.Remove(model);
			}
		}

		private void SaveItems() {
			foreach (var item in _items) {
				SaveModel(item.Value);
			}
		}

		private void DeleteItems() {
			foreach (var item in _itemsToDelete) {
				DeleteModel(item);
			}
		}

		#endregion

		#region Methods: Public

		public T GetItem<T>(Guid id) where T : BaseModel {
			var method = GetLoadModelMethod(typeof(T));
			var item = method.Invoke(this, new object[] { id });
			return (T)item;
		}

		public List<T> GetItems<T>(string filterPropertyName, Guid filterValue) where T : BaseModel {
			var method = GetLoadModelListMethod(typeof(T));
			var list = method.Invoke(this, new object[] { filterPropertyName, filterValue });
			return (List<T>)list;
		}

		public T CreateItem<T>() where T : BaseModel {
			MethodInfo method = GetGenericMethod(typeof(T), "CreateModelById");
			var model = method.Invoke(this, new object[] { Guid.Empty });
			return model != null
				? (T)model
				: default;
		}

		public void DeleteItem<T>(T model) where T : BaseModel {
			if (!_items.ContainsKey(model.Id) || !_entities.ContainsKey(model.Id) || _itemsToDelete.Contains(model)) {
				// TODO #3. Добавить корректную обработку ошибок.
				throw new KeyNotFoundException();
			}
			_items.Remove(model.Id);
			_entities.Remove(model.Id);
			_itemsToDelete.Add(model);
		}

		public void Save() {
			if (!DataStoreEnabled) {
				return;
			}
			SaveItems();
			DeleteItems();
		}

		#endregion
	}

}
