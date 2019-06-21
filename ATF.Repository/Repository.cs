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
			var modelId = ConvertValue<Guid>(values[DefaultPrimaryColumnName]);
			if (_items.ContainsKey(modelId)) {
				return (T)_items[modelId];
			}
			return CreateItem<T>(values);
		}

		private T CreateItem<T>(IDictionary<string, object> values) where T : BaseModel, new() {
			var model = CreateModel<T>();
			FillPropertyValues<T>(model, values);
			FillLookupLinkValues<T>(model, values);
			_items.Add(model.Id, model);
			FillReferenceValues<T>(model);
			FillLookupValues<T>(model);
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
			var esq = GetValuesESQ<T>(filter);
			var collection = esq.GetEntityCollection(UserConnection);
			collection.ForEach(entity => {
				var values = GetValuesFromEntity<T>(entity);
				response.Add(values);
			});
			return response;
		}

		private EntitySchemaQuery GetValuesESQ<T>(Filter filter) where T : BaseModel, new() {
			Type type = typeof(T);
			string entitySchemaName = GetEntitySchemaName(type);
			var esq = new EntitySchemaQuery(UserConnection.EntitySchemaManager, entitySchemaName);
			var filterColumn = esq.RootSchema.Columns
				.Where(x => x.Name == filter.Name || x.ColumnValueName == filter.Name)
				.FirstOrDefault();
			esq.UseAdminRights = UseAdminRight;
			esq.PrimaryQueryColumn.IsAlwaysSelect = true;

			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, filterColumn.Name, filter.Value));
			_modelMapper.GetProperties(type)
				.Where(x => !x.IsLazy && x.EntityColumnName != DefaultPrimaryColumnName)
				.ForEach(x => esq.AddColumn(x.EntityColumnName));
			_modelMapper.GetLookups(type).ForEach(x => {
				if (!esq.Columns.Where(c => c.Name == x.EntityColumnName).Any()) {
					esq.AddColumn(x.EntityColumnName);
				}
			});
			return esq;
		}

		private string GetColumnNameByPropertyName(Type type, ModelItem property) {
			var entitySchema = GetEntitySchema(type);
			var schemaColumn = entitySchema?.Columns.GetByName(property.EntityColumnName);
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
			var properies = _modelMapper.GetProperties(model.GetType());
			foreach (var property in properies) {
				var value = values.ContainsKey(property.PropertyName)
					? values[property.PropertyName]
					: null;
				FillPropertyValue<T>(model, property.PropertyInfo, value);
			}
			UpdateInitValues<T>(model);
		}

		private void FillPropertyValue<T>(T model, PropertyInfo propertyInfo, object value) {
			if (propertyInfo != null && (value != null || Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null)) {
				propertyInfo.SetValue(model, Convert.ChangeType(value, propertyInfo.PropertyType), null);
			}
		}

		private void FillLookupLinkValues<T>(T model, IDictionary<string, object> values) where T : BaseModel {
			var properties = _modelMapper.GetProperties(model.GetType());
			var lookups = _modelMapper.GetLookups(model.GetType())
				.Where(lookup => !properties.Any(property => property.EntityColumnName == lookup.EntityColumnName));

			foreach (var lookup in lookups) {
				var value = values.ContainsKey(lookup.PropertyName)
					? values[lookup.PropertyName]
					: null;
				SetLookupLinkValue<T>(model, lookup, value);
			}
		}

		private void SetLookupLinkValue<T>(T model, ModelItem lookup, object value) where T : BaseModel {
			if (lookup.PropertyInfo == null || value == null) {
				return;
			}
			var lazyLookupKey = GetLazyLookupKey(lookup.PropertyInfo.Name);
			if (model.LazyValues.ContainsKey(lazyLookupKey)) {
				model.LazyValues[lazyLookupKey] = Convert.ChangeType(value, typeof(Guid));
			} else {
				model.LazyValues.Add(lazyLookupKey, Convert.ChangeType(value, typeof(Guid)));
			}
		}

		private string GetLazyLookupKey(string propertyName) {
			return $"Lookup::{propertyName}";
		}

		private T GetModelPropertyValue<T>(BaseModel model, string propertyName) {
			var property = model.GetType().GetProperty(propertyName);
			object value = null;
			if (property != null && property.PropertyType == typeof(T)) {
				value = property.GetValue(model);
			}
			return ConvertValue<T>(value);
		}

		private T GetLookupLinkValue<T>(BaseModel model, ModelItem lookup) {
			var property = _modelMapper
				.GetProperties(model.GetType())
				.Where(x => x.EntityColumnName == lookup.EntityColumnName)
				.FirstOrDefault();
			return (property != null)
				? (T)property.PropertyInfo.GetValue(model)
				: GetModelLazyValue<T>(model, GetLazyLookupKey(lookup.PropertyName));
		}

		private T GetModelLazyValue<T>(BaseModel model, string key) {
			return model.LazyValues.ContainsKey(key)
				? ConvertValue<T>(model.LazyValues[key])
				: default(T);
		}

		private T ConvertValue<T>(object value) {
			return value != null && value.GetType() == typeof(T)
				? (T)value
				: default(T);
		}

		[Obsolete("Will be removed in 1.3.0")]
		private void FillReferenceValues<T>(T model) where T : BaseModel, new() {
			var references = _modelMapper.GetReferences(model.GetType());
			foreach (var reference in references.Where(x => !x.IsLazy)) {
				FillReferenceValue<T>(model, reference);
			}
		}

		[Obsolete("Will be removed in 1.3.0")]
		internal void FillReferenceValue<T>(T model, ModelItem reference) where T : BaseModel {
			var referenceId = GetModelPropertyValue<Guid>(model, reference.EntityColumnName);
			if (referenceId != Guid.Empty && reference.PropertyInfo != null) {
				var method = GetGenericMethod(GetType(), reference.DataValueType, "GetItem");
				reference.PropertyInfo.SetValue(model, method.Invoke(this, new object[] { referenceId }));
			}
		}

		private void FillLookupValues<T>(T model) where T : BaseModel, new() {
			_modelMapper
				.GetLookups(model.GetType())
				.Where(x => !x.IsLazy)
				.ForEach(lookup => FillLookupValue<T>(model, lookup));
		}

		internal void FillLookupValue<T>(T model, ModelItem lookup) where T : BaseModel {
			var lookupId = GetLookupLinkValue<Guid>(model, lookup);
			if (lookupId != Guid.Empty && lookup.PropertyInfo != null) {
				var method = GetGenericMethod(GetType(), lookup.DataValueType, "GetItem");
				lookup.PropertyInfo.SetValue(model, method.Invoke(this, new object[] { lookupId }));
			}
		}

		private void FillDetailValues<T>(T model) where T : BaseModel {
			var details = _modelMapper.GetDetails(model.GetType());
			foreach (var detail in details.Where(x => !x.IsLazy)) {
				FillDetailValue<T>(model, detail);
			}
		}

		internal void FillDetailValue<T>(T model, ModelItem detail) where T : BaseModel {
			var masterFilterPropertyName = !string.IsNullOrEmpty(detail.MasterModelPropertyName)
				? detail.MasterModelPropertyName
				: DefaultPrimaryColumnName;
			var masterId = GetModelPropertyValue<Guid>(model, masterFilterPropertyName);
			if (detail.PropertyInfo != null) {
				var method = GetGenericMethod(GetType(), detail.DataValueType, "GetItems");
				detail.PropertyInfo.SetValue(model, method.Invoke(this, new object[] { detail.DetailModelPropertyName, masterId }));
			}
		}

		private IDictionary<string, object> GetValuesFromEntity<T>(Entity entity) where T : BaseModel, new() {
			var response = new Dictionary<string, object>();
			if (entity != null) {
				response.AddRangeIfNotExists(GetValuesFromEntity<T>(entity, _modelMapper.GetProperties(typeof(T))));
				response.AddRangeIfNotExists(GetValuesFromEntity<T>(entity, _modelMapper.GetLookups(typeof(T))));
			} else {
				response.Add(DefaultPrimaryColumnName, Guid.NewGuid());
			}
			return response;
		}

		private IDictionary<string, object> GetValuesFromEntity<T>(Entity entity, List<ModelItem> modelItems) where T : BaseModel, new() {
			var response = new Dictionary<string, object>();
			foreach (var modelItem in modelItems) {
				var schemaColumn = entity.Schema.Columns.GetByName(modelItem.EntityColumnName);
				response.Add(modelItem.PropertyName, entity.GetColumnValue(schemaColumn.ColumnValueName));
			}
			return response;
		}

		private IDictionary<string, object> GetValuesFromModel<T>(T model) where T : BaseModel {
			var response = new Dictionary<string, object>();
			_modelMapper.GetProperties(model.GetType()).ForEach(x => {
				response.Add(x.PropertyName, x.PropertyInfo.GetValue(model));
			});
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
			_modelMapper.GetProperties(model.GetType()).ForEach(parameter => {
				if (valuesToSave.ContainsKey(parameter.PropertyName)) {
					var value = PrepareModelValueForSave(valuesToSave[parameter.PropertyName], parameter.DataValueType);
					entity.SetColumnValue(GetColumnNameByPropertyName(model.GetType(), parameter), value);
				}
			});
			entity.UseAdminRights = UseAdminRight;
			entity.Save(false);
			PrepareModelAfterSave(model);
		}

		private void PrepareModelAfterSave<T>(T model) where T : BaseModel {
			model.IsNew = false;
			UpdateInitValues(model);
		}

		private void UpdateInitValues<T>(T model) where T : BaseModel {
			model.InitValues.Clear();
			var parameters = _modelMapper.GetProperties(model.GetType());
			var currentModelValues = GetValuesFromModel<T>(model);
			parameters.ForEach(parameter => {
				model.InitValues.Add(parameter.PropertyName, currentModelValues[parameter.PropertyName]);
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
			var parameters = _modelMapper.GetProperties(model.GetType());
			var currentModelValues = GetValuesFromModel<T>(model);
			parameters.ForEach(parameter => {
				if (GetIsItemExistsAndNotEqual(model.InitValues, currentModelValues, parameter.PropertyName)) {
					response.Add(parameter.PropertyName, currentModelValues[parameter.PropertyName]);
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
			entity.Delete();
		}

		private void SaveItems() {
			foreach (var item in _items.Where(x => !x.Value.IsMarkAsDeleted)) {
				SaveModel(item.Value);
			}
		}

		private void DeleteItems() {
			var toDelete = _items.Where(x=>x.Value.IsMarkAsDeleted).ToArray();
			foreach (var item in toDelete) {
				DeleteModel(item.Value);
				_items.Remove(item.Key);
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
			if (!_items.ContainsKey(model.Id)) {
				// TODO #3. Добавить корректную обработку ошибок.
				throw new KeyNotFoundException();
			}
			model.IsMarkAsDeleted = true;
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
