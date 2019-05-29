namespace ATF.Repository
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Reflection;
	using ATF.Repository.Attributes;
	using ATF.Repository.Builder;
	using ATF.Repository.Mapping;
	using Terrasoft.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.DB;
	using Terrasoft.Core.Entities;
	using Terrasoft.Core.Factories;

	[DefaultBinding(typeof(IRepository))]
	public class Repository : IRepository
	{
		class Filter
		{
			public string Name { get; set; }

			public Guid Value { get; set; }
		}

		#region Fields: Private

		private Dictionary<Guid, BaseModel> _items;
		private ModelMapper _modelMapper;
		private ProxyClassBuilder _proxyClassBuilder;
		private List<BaseModel> _itemsToDelete;

		private static string DefaultPrimaryColumnName = "Id";

		#endregion

		#region Properties: Protected

		public bool DataStoreEnabled {
			get {
				return UserConnection != null;
			}
		}

		#endregion

		#region Properties: Public

		public bool UseAdminRight { get; set; }

		public UserConnection UserConnection { private get; set; }

		#endregion

		#region Constructors: Public

		public Repository() {
			_items = new Dictionary<Guid, BaseModel>();
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

		private T LoadModelByValues<T>(IDictionary<string, object> values) where T : BaseModel, new() {
			if (!values.ContainsKey(DefaultPrimaryColumnName)) {
				return null;
			}
			var modelId = ConvertToGuid(values[DefaultPrimaryColumnName]);
			if (_items.ContainsKey(modelId)) {
				return (T)_items[modelId];
			}
			return CreateItem<T>(values);
		}

		private T CreateItem<T>(IDictionary<string, object> values) where T : BaseModel, new() {
			var model = CreateModel<T>();
			FillPropertyValues(model, values);
			_items.Add(model.Id, model);
			FillReferenceValues<T>(model);
			FillDetailValues<T>(model);
			return model;
		}

		private T CreateItem<T>(Entity entity) where T : BaseModel, new() {
			var values = GetValuesFromEntity<T>(entity);
			var model = CreateItem<T>(values);
			model.Entity = entity;
			model.IsNew = true;
			return model;
		}

		private List<IDictionary<string, object>> GetRecordsValues<T>(Filter filter) where T : BaseModel, new() {
			List<IDictionary<string, object>> response = new List<IDictionary<string, object>>();
			if (!DataStoreEnabled || string.IsNullOrEmpty(filter.Name) || filter.Value == Guid.Empty) {
				return response;
			}
			Type type = typeof(T);
			var select = GetRecordsValuesSelect<T>(filter);
			select.ExecuteReader(dataReader => {
				var values = GetRecordValuesFromDataReader<T>(dataReader);
				response.Add(values);
			});
			return response;
		}

		private Select GetRecordsValuesSelect<T>(Filter filter) where T : BaseModel, new() {
			Type type = typeof(T);
			string entitySchemaName = GetEntitySchemaName(type);
			var parameters = _modelMapper.GetParameters(type);
			var filterColumnName = GetColumnNameByPropertyName(type, filter.Name);
			var select = new Select(UserConnection)
				.From(entitySchemaName)
				.Where(filterColumnName).IsEqual(Column.Parameter(filter.Value)) as Select;
			parameters.ForEach(parameter => {
				select = select.Column(GetColumnNameByPropertyName(type, parameter.Name)) as Select;
			});
			return select;
		}

		private IDictionary<string, object> GetRecordValuesFromDataReader<T>(IDataReader dataReader) where T : BaseModel, new() {
			var values = new Dictionary<string, object>();
			Type type = typeof(T);
			var parameters = _modelMapper.GetParameters(type);
			parameters.ForEach(parameter => {
				var method = GetGenericMethod(typeof(DBUtilities), parameter.Type, "GetColumnValue");
				var value = method.Invoke(dataReader, new object[] { dataReader, GetColumnNameByPropertyName(type, parameter.Name) });
				values.Add(parameter.Name, value);
			});
			return values;
		}

		private string GetColumnNameByPropertyName(Type type, string propertyName) {
			var mapper = _modelMapper.GetParameters(type).Where(x => x.Name == propertyName).FirstOrDefault();
			var entitySchema = GetEntitySchema(type);
			var schemaColumn = entitySchema?.Columns.GetByName(mapper.EntitySchemaColumnName);
			return schemaColumn != null
				? schemaColumn.ColumnValueName
				: string.Empty;
		}

		private MethodInfo GetGenericMethod(Type type, Type genericType, string methodName) {
			MethodInfo response = type.GetMethods().Where(method => method.Name == methodName && method.ContainsGenericParameters).FirstOrDefault();
			return response?.MakeGenericMethod(genericType);
		}

		private T CreateModel<T>() where T : BaseModel, new() {
			var model = _proxyClassBuilder.Build<T>();
			model.UserConnection = UserConnection;
			return model;
		}

		private void FillPropertyValues<T>(T model, IDictionary<string, object> values) where T : BaseModel {
			foreach (var item in values) {
				model.InitValues.Add(item);
				PropertyInfo propertyInfo = model.GetType().GetProperty(item.Key);
				if (item.Value != null || Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null) {
					propertyInfo.SetValue(model, Convert.ChangeType(item.Value, propertyInfo.PropertyType), null);
				}
			}
		}

		private Guid GetReferenceValue<T>(T model, string propertyName) where T : BaseModel {
			var property = model.GetType().GetProperty(propertyName);
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

		private void FillReferenceValues<T>(T model) where T : BaseModel, new() {
			var references = _modelMapper.GetReferences(model.GetType());
			foreach (var reference in references.Where(x => !x.IsLazyLoad)) {
				FillReferenceValue<T>(model, reference);
			}
		}

		internal void FillReferenceValue<T>(T model, ModelReference reference) where T : BaseModel {
			if (!DataStoreEnabled) {
				return;
			}
			var property = model.GetType().GetProperty(reference.Name);
			var referenceId = GetReferenceValue<T>(model, reference.ValuePropertyName);
			if (referenceId != Guid.Empty && property != null) {
				var method = GetGenericMethod(GetType(), reference.Type, "GetItem");
				property.SetValue(model, method.Invoke(this, new object[] { referenceId }));
			}
		}

		private void FillDetailValues<T>(T model) where T : BaseModel {
			var details = _modelMapper.GetDetails(model.GetType());
			foreach (var detail in details.Where(x => !x.IsLazyLoad)) {
				FillDetailValue<T>(model, detail);
			}
		}

		internal void FillDetailValue<T>(T model, ModelDetail detail) where T : BaseModel {
			var valueProperty = model.GetType().GetProperty(detail.Name);
			var masterFilterPropertyName = !string.IsNullOrEmpty(detail.MasterFilterPropertyName)
				? detail.MasterFilterPropertyName
				: DefaultPrimaryColumnName;
			var masterId = GetReferenceValue<T>(model, masterFilterPropertyName);
			if (valueProperty != null) {
				var method = GetGenericMethod(GetType(), detail.Type, "GetItems");
				valueProperty.SetValue(model, method.Invoke(this, new object[] { detail.DetailFilterPropertyName, masterId }));
			}
		}

		private IDictionary<string, object> GetValuesFromEntity<T>(Entity entity) where T : BaseModel, new() {
			var response = new Dictionary<string, object>();
			if (entity != null) {
				var parameters = _modelMapper.GetParameters(typeof(T));
				foreach (var item in parameters) {
					var schemaColumn = entity.Schema.Columns.GetByName(item.EntitySchemaColumnName);
					response.Add(item.Name, entity.GetColumnValue(schemaColumn.ColumnValueName));
				}
			} else {
				response.Add(DefaultPrimaryColumnName, Guid.NewGuid());
			}
			return response;
		}

		private IDictionary<string, object> GetValuesFromModel<T>(T model) where T : BaseModel {
			var response = new Dictionary<string, object>();
			var parameters = _modelMapper.GetParameters(model.GetType());
			foreach (var parameter in parameters) {
				var propertyInfo = model.GetType().GetProperty(parameter.Name);
				response.Add(parameter.Name, propertyInfo.GetValue(model));
			}
			return response;
		}

		private Entity LoadEntity(Type type, Guid id) {
			var schema = GetEntitySchema(type);
			var entity = schema?.CreateEntity(UserConnection);
			return entity != null && entity.FetchFromDB(id)
				? entity
				: null;
		}

		private Entity CreateEntity(Type type) {
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

		private void SaveModel<T>(T model) where T : BaseModel {
			var valuesToSave = GetValuesForSave<T>(model);
			if (!valuesToSave.Any()) {
				return;
			}
			var entity = GetModelEntity<T>(model);
			if (entity == null) {
				return;
			}
			var parameters = _modelMapper.GetParameters(model.GetType());
			parameters.ForEach(parameter => {
				if (valuesToSave.ContainsKey(parameter.Name)) {
					var value = PrepareModelValueForSave(valuesToSave[parameter.Name], parameter.Type);
					entity.SetColumnValue(GetColumnNameByPropertyName(model.GetType(), parameter.Name), value);
				}
			});
			entity.UseAdminRights = UseAdminRight;
			entity.Save(false);
			PrepareModelAfterSave(model);
		}

		private void PrepareModelAfterSave<T>(T model) where T : BaseModel {
			model.IsNew = false;
			model.InitValues.Clear();
			var parameters = _modelMapper.GetParameters(model.GetType());
			var currentModelValues = GetValuesFromModel<T>(model);
			parameters.ForEach(parameter => {
				model.InitValues.Add(parameter.Name, currentModelValues[parameter.Name]);
			});
		}

		private Entity GetModelEntity<T>(T model) where T : BaseModel {
			return model.Entity ?? LoadEntity(model.GetType(), model.Id);
		}

		private IDictionary<string, object> GetValuesForSave<T>(T model) where T : BaseModel {
			return model.IsNew
				? GetValuesFromModel<T>(model)
				: GetChangedValues<T>(model);
		}

		private IDictionary<string, object> GetChangedValues<T>(T model) where T : BaseModel {
			var response = new Dictionary<string, object>();
			var parameters = _modelMapper.GetParameters(model.GetType());
			var currentModelValues = GetValuesFromModel<T>(model);
			parameters.ForEach(parameter => {
				if (GetIsItemExistsAndNotEqual(model.InitValues, currentModelValues, parameter.Name)) {
					response.Add(parameter.Name, currentModelValues[parameter.Name]);
				}
			});
			return response;
		}

		private bool GetIsItemExistsAndNotEqual(IDictionary<string, object> oldValues, IDictionary<string, object> newValues, string key) {
			return oldValues.ContainsKey(key) && newValues.ContainsKey(key) &&
					!Equals(oldValues[key], newValues[key]);
		}

		private object PrepareModelValueForSave(object value, Type type) {
			if ((type == typeof(DateTime) && (DateTime)value == DateTime.MinValue) ||
				(type == typeof(Guid) && (Guid)value == Guid.Empty)) {
				return null;
			}
			return value;
		}

		private void DeleteModel<T>(T model) where T : BaseModel {
			var entity = LoadEntity(model.GetType(), model.Id);
			if (entity == null) {
				return;
			}
			entity.UseAdminRights = UseAdminRight;
			if (model.IsNew || entity.Delete()) {
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
			var toDelete = _itemsToDelete.ToArray();
			foreach (var item in toDelete) {
				DeleteModel(item);
			}
		}

		#endregion

		#region Methods: Public

		public T GetItem<T>(Guid id) where T : BaseModel, new() {
			if (_items.ContainsKey(id)) {
				return (T)_items[id];
			}
			var items = GetItems<T>(DefaultPrimaryColumnName, id);
			return items.Count > 0
				? items.First()
				: null;
		}

		public List<T> GetItems<T>(string filterPropertyName, Guid filterValue) where T : BaseModel, new() {
			var response = new List<T>();
			var recordsValues = GetRecordsValues<T>(new Filter() { Name = filterPropertyName, Value = filterValue });
			recordsValues.ForEach(recordValues => {
				var model = LoadModelByValues<T>(recordValues);
				if (model != null) {
					response.Add(model);
				}
			});
			return response;
		}

		public T CreateItem<T>() where T : BaseModel, new() {
			var entity = CreateEntity(typeof(T));
			return CreateItem<T>(entity);
		}

		public void DeleteItem<T>(T model) where T : BaseModel {
			if (!_items.ContainsKey(model.Id) || _itemsToDelete.Contains(model)) {
				// TODO #3. Добавить корректную обработку ошибок.
				throw new KeyNotFoundException();
			}
			_items.Remove(model.Id);
			_itemsToDelete.Add(model);
		}

		public void Save() {
			if (!DataStoreEnabled) {
				return;
			}
			DeleteItems();
			SaveItems();
		}

		#endregion
	}

}
