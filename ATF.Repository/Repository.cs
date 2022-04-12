namespace ATF.Repository
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Attributes;
	using Builder;
	using Mapping;
	using Terrasoft.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.Entities;
	using Terrasoft.Core.Factories;
	using Common.Logging;

	[DefaultBinding(typeof(IRepository))]
	public class Repository : IRepository
	{
		class Filter
		{
			public string EntityColumnName { get; set; }

			public Guid EntityColumnValue { get; set; }
		}

		#region Fields: Private

		internal readonly Dictionary<Guid, BaseModel> Items;
		private readonly ProxyClassBuilder _proxyClassBuilder;

		private static string DefaultPrimaryEntityColumnName = "Id";
		private static readonly ILog _log = LogManager.GetLogger("Terrasoft");

		#endregion

		#region Properties: Public

		public bool UseAdminRight { get; set; }

		public UserConnection UserConnection { private get; set; }
		public IChangeTracker ChangeTracker { get; private set; }
		private bool DataStoreEnabled => UserConnection != null;

		#endregion

		#region Constructors: Public

		public Repository() {
			Items = new Dictionary<Guid, BaseModel>();
			_proxyClassBuilder = new ProxyClassBuilder(this);
			ChangeTracker = new RepositoryChangeTracker { Repository = this };
		}

		#endregion

		#region Methods: Private

		private void LogEvent(string message) {
			_log.Error(message);
		}

		private static string GetEntitySchemaName(MemberInfo type) {
			string name = string.Empty;
			if (Attribute.IsDefined(type, typeof(SchemaAttribute)) &&
				(type.GetCustomAttribute(typeof(SchemaAttribute)) as SchemaAttribute) != null) {
				SchemaAttribute attribute = type.GetCustomAttribute(typeof(SchemaAttribute)) as SchemaAttribute;
				name = attribute?.Name;
			}
			return name;
		}

		private T LoadModelByValues<T>(IDictionary<string, object> values) where T : BaseModel, new() {
			if (!values.ContainsKey(DefaultPrimaryEntityColumnName)) {
				return null;
			}
			var modelId = ConvertValue<Guid>(values[DefaultPrimaryEntityColumnName]);
			if (Items.ContainsKey(modelId)) {
				return (T)Items[modelId];
			}
			return CreateItem<T>(values);
		}

		private T CreateItem<T>(IDictionary<string, object> values) where T : BaseModel, new() {
			var model = CreateModel<T>();
			FillPropertyValues<T>(model, values);
			FillLookupLinkValues<T>(model, values);
			Items.Add(model.Id, model);
			FillLookupValues<T>(model);
			FillDetailValues<T>(model);
			return model;
		}

		private T CreateItem<T>(Entity entity) where T : BaseModel, new() {
			var values = GetValuesFromEntity<T>(entity);
			var model = CreateItem<T>(values);
			model.InternalEntity = entity;
			model.IsNew = true;
			return model;
		}

		private List<IDictionary<string, object>> GetRecordsValues<T>(Filter filter) where T : BaseModel, new() {
			List<IDictionary<string, object>> response = new List<IDictionary<string, object>>();
			if (!DataStoreEnabled || string.IsNullOrEmpty(filter.EntityColumnName) || filter.EntityColumnValue == Guid.Empty) {
				return response;
			}
			try {
				var esq = GetValuesEsq<T>(filter);
				var collection = esq.GetEntityCollection(UserConnection);
				collection.ForEach(entity => {
					var values = GetValuesFromEntity<T>(entity);
					response.Add(values);
				});
			} catch (Exception e) {
				LogEvent(
					$"GetRecordsValues. DetailLinkPropertyName: {filter.EntityColumnName}, MasterId: {filter.EntityColumnValue}. \n ErrorMessage: {e.Message}. At: {e.StackTrace}");
				throw;
			}
			return response;
		}

		private EntitySchemaQuery GetValuesEsq<T>(Filter filter) where T : BaseModel, new() {
			Type type = typeof(T);
			string entitySchemaName = GetEntitySchemaName(type);
			var esq = new EntitySchemaQuery(UserConnection.EntitySchemaManager, entitySchemaName);
			try {
				var filterColumn = esq.RootSchema.Columns
					.FirstOrDefault(x => x.Name == filter.EntityColumnName || x.ColumnValueName == filter.EntityColumnName);
				esq.UseAdminRights = UseAdminRight;
				esq.PrimaryQueryColumn.IsAlwaysSelect = true;

				if (filterColumn != null) {
					esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, filterColumn.Name,
						filter.EntityColumnValue));
				} else {
					throw new ArgumentException();
				}
				ModelMapper.GetProperties(type)
					.Where(x => !x.IsLazy && x.EntityColumnName != DefaultPrimaryEntityColumnName)
					.ForEach(x => {
						try {
							esq.AddColumn(x.EntityColumnName);
						} catch (Exception e) {
							LogEvent(
								$"AddESQColumn. EntityColumnName: {x.EntityColumnName}, \n ErrorMessage: {e.Message}. At: {e.StackTrace}");
							throw;
						}
					});
				ModelMapper.GetLookups(type).ForEach(x => {
					try {
						if (esq.Columns.All(c => c.Name != x.EntityColumnName)) {
							esq.AddColumn(x.EntityColumnName);
						}
					} catch (Exception e) {
						LogEvent(
							$"AddESQLookupColumn. EntityColumnName: {x.EntityColumnName}, \n ErrorMessage: {e.Message}. At: {e.StackTrace}");
						throw;
					}
				});
			} catch (Exception e) {
				LogEvent(
					$"GetRecordsValues. DetailLinkPropertyName: {filter.EntityColumnName}, MasterId: {filter.EntityColumnValue}. \n ErrorMessage: {e.Message}. At: {e.StackTrace}");
				throw;
			}
			return esq;
		}

		private string GetColumnNameByPropertyName(Type type, ModelItem property) {
			var entitySchema = GetEntitySchema(type);
			var schemaColumn = entitySchema?.Columns.GetByName(property.EntityColumnName);
			return schemaColumn != null
				? schemaColumn.ColumnValueName
				: string.Empty;
		}

		private T CreateModel<T>() where T : BaseModel, new() {
			var model = _proxyClassBuilder.Build<T>();
			model.UserConnection = UserConnection;
			return model;
		}

		private void FillPropertyValues<T>(T model, IDictionary<string, object> values) where T : BaseModel {
			var properties = ModelMapper.GetProperties(model.GetType());
			foreach (var property in properties) {
				var value = values.ContainsKey(property.PropertyName)
					? values[property.PropertyName]
					: null;
				FillPropertyValue(model, property.PropertyInfo, value);
			}
			UpdateInitValues(model);
		}

		private void FillPropertyValue<T>(T model, PropertyInfo propertyInfo, object value) {
			if (propertyInfo != null && (value != null || Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null)) {
				propertyInfo.SetValue(model, Convert.ChangeType(value, propertyInfo.PropertyType), null);
			}
		}

		private void FillLookupLinkValues<T>(T model, IDictionary<string, object> values) where T : BaseModel {
			var properties = ModelMapper.GetProperties(model.GetType());
			var lookups = ModelMapper.GetLookups(model.GetType())
				.Where(lookup => properties.All(property => property.EntityColumnName != lookup.EntityColumnName));

			foreach (var lookup in lookups) {
				var value = values.ContainsKey(lookup.PropertyName)
					? values[lookup.PropertyName]
					: null;
				SetLookupLinkValue(model, lookup, value);
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

		private static string GetLazyLookupKey(string propertyName) {
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
			var property = ModelMapper
				.GetProperties(model.GetType())
				.FirstOrDefault(x => x.EntityColumnName == lookup.EntityColumnName);
			return property != null
				? (T)property.PropertyInfo.GetValue(model)
				: GetModelLazyValue<T>(model, GetLazyLookupKey(lookup.PropertyName));
		}

		private T GetModelLazyValue<T>(BaseModel model, string key) {
			return model.LazyValues.ContainsKey(key)
				? ConvertValue<T>(model.LazyValues[key])
				: default;
		}

		private T ConvertValue<T>(object value) {
			return value != null && value.GetType() == typeof(T)
				? (T)value
				: default;
		}

		[Obsolete("Will be removed in 1.3.0")]
		private void FillReferenceValues<T>(T model) where T : BaseModel, new() {
			var references = ModelMapper.GetReferences(model.GetType());
			foreach (var reference in references.Where(x => !x.IsLazy)) {
				FillReferenceValue(model, reference);
			}
		}

		[Obsolete("Will be removed in 1.3.0")]
		internal void FillReferenceValue<T>(T model, ModelItem reference) where T : BaseModel {
			var referenceId = GetModelPropertyValue<Guid>(model, reference.EntityColumnName);
			if (referenceId != Guid.Empty && reference.PropertyInfo != null) {
				var method = RepositoryReflectionUtilities.GetGenericMethod(GetType(), "GetItem", reference.DataValueType);
				reference.PropertyInfo.SetValue(model, method.Invoke(this, new object[] { referenceId }));
			}
		}

		private void FillLookupValues<T>(T model) where T : BaseModel, new() {
			ModelMapper
				.GetLookups(model.GetType())
				.Where(x => !x.IsLazy)
				.ForEach(lookup => FillLookupValue(model, lookup));
		}

		internal void FillLookupValue<T>(T model, ModelItem lookup) where T : BaseModel {
			var lookupId = GetLookupLinkValue<Guid>(model, lookup);
			if (lookupId != Guid.Empty && lookup.PropertyInfo != null) {
				var method = RepositoryReflectionUtilities.GetGenericMethod(GetType(), "GetItem", lookup.DataValueType);
				lookup.PropertyInfo.SetValue(model, method.Invoke(this, new object[] { lookupId }));
			}
		}

		private void FillDetailValues<T>(T model) where T : BaseModel {
			var details = ModelMapper.GetDetails(model.GetType());
			foreach (var detail in details.Where(x => !x.IsLazy)) {
				FillDetailValue(model, detail);
			}
		}

		internal void FillDetailValue<T>(T model, ModelItem detail) where T : BaseModel {
			var masterFilterPropertyName = !string.IsNullOrEmpty(detail.MasterEntityColumnName)
				? detail.MasterEntityColumnName
				: DefaultPrimaryEntityColumnName;
			var masterId = GetModelPropertyValue<Guid>(model, masterFilterPropertyName);
			if (detail.PropertyInfo != null) {
				var method = RepositoryReflectionUtilities.GetGenericMethod(GetType(), "GetItems", detail.DataValueType);
				detail.PropertyInfo.SetValue(model, method.Invoke(this, new object[] { detail.DetailLinkPropertyName, masterId }));
			}
		}

		private IDictionary<string, object> GetValuesFromEntity<T>(Entity entity) where T : BaseModel, new() {
			var response = new Dictionary<string, object>();
			if (entity != null) {
				response.AddRangeIfNotExists(GetValuesFromEntity(entity, ModelMapper.GetProperties(typeof(T))));
				response.AddRangeIfNotExists(GetValuesFromEntity(entity, ModelMapper.GetLookups(typeof(T))));
			} else {
				response.Add(DefaultPrimaryEntityColumnName, Guid.NewGuid());
			}
			return response;
		}

		private IDictionary<string, object> GetValuesFromEntity(Entity entity, IEnumerable<ModelItem> modelItems) {
			var response = new Dictionary<string, object>();
			foreach (var modelItem in modelItems) {
				try {
					var schemaColumn = entity.Schema.Columns.GetByName(modelItem.EntityColumnName);
					response.Add(modelItem.PropertyName, entity.GetColumnValue(schemaColumn.ColumnValueName));
				} catch (Exception e) {
					LogEvent(
						$"GetValuesFromEntity.  EntityName: {entity.Schema.Name}, EntityColumnName: {modelItem.EntityColumnName}, \n ErrorMessage: {e.Message}. At: {e.StackTrace}");
					throw;
				}
				
			}
			return response;
		}

		private IDictionary<string, object> GetValuesFromModel<T>(T model) where T : BaseModel {
			var response = new Dictionary<string, object>();
			ModelMapper.GetProperties(model.GetType()).ForEach(x => {
				response.Add(x.PropertyName, x.PropertyInfo.GetValue(model));
			});
			return response;
		}

		private Entity LoadEntity(Type type, Guid id) {
			var schema = GetEntitySchema(type);
			var entity = schema?.CreateEntity(UserConnection);
			if (entity != null) {
				entity.UseAdminRights = UseAdminRight;
				return entity.FetchFromDB(id) ? entity : null;
			} else {
				return null;
			}
		}

		private Entity CreateEntity(Type type) {
			var schema = GetEntitySchema(type);
			Entity entity = schema?.CreateEntity(UserConnection);
			if (schema != null) {
				entity.UseAdminRights = UseAdminRight;
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
			var valuesToSave = GetValuesForSave(model);
			if (!valuesToSave.Any()) {
				return;
			}
			var entity = GetModelEntity(model);
			if (entity == null) {
				return;
			}
			ModelMapper.GetProperties(model.GetType()).ForEach(parameter => {
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
			var parameters = ModelMapper.GetProperties(model.GetType());
			var currentModelValues = GetValuesFromModel(model);
			parameters.ForEach(parameter => {
				model.InitValues.Add(parameter.PropertyName, currentModelValues[parameter.PropertyName]);
			});
		}

		private Entity GetModelEntity<T>(T model) where T : BaseModel {
			return model.InternalEntity ?? LoadEntity(model.GetType(), model.Id);
		}

		private IDictionary<string, object> GetValuesForSave<T>(T model) where T : BaseModel {
			return model.IsNew
				? GetValuesFromModel(model)
				: GetChangedValues(model);
		}

		private IDictionary<string, object> GetChangedValues<T>(T model) where T : BaseModel {
			var response = new Dictionary<string, object>();
			var parameters = ModelMapper.GetProperties(model.GetType());
			var currentModelValues = GetValuesFromModel(model);
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
			foreach (var item in Items.Where(x => !x.Value.IsMarkAsDeleted)) {
				SaveModel(item.Value);
			}
		}

		private void DeleteItems() {
			var toDelete = Items.Where(x=>x.Value.IsMarkAsDeleted).ToArray();
			foreach (var item in toDelete) {
				DeleteModel(item.Value);
				Items.Remove(item.Key);
			}
		}

		#endregion

		#region Methods: Public

		public T GetItem<T>(Guid id) where T : BaseModel, new() {
			if (Items.ContainsKey(id)) {
				return (T)Items[id];
			}
			var items = GetItems<T>(DefaultPrimaryEntityColumnName, id);
			return items.Count > 0
				? items.First()
				: null;
		}

		public List<T> GetItems<T>(string filterPropertyName, Guid filterValue) where T : BaseModel, new() {
			var response = new List<T>();
			try {
				var recordsValues = GetRecordsValues<T>(new Filter() { EntityColumnName = filterPropertyName, EntityColumnValue = filterValue });
				recordsValues.ForEach(recordValues => {
					var model = LoadModelByValues<T>(recordValues);
					if (model != null) {
						response.Add(model);
					}
				});
			} catch (Exception e) {
				LogEvent(
					$"GetItems. DetailLinkPropertyName: {filterPropertyName}, MasterId: {filterValue}. \n ErrorMessage: {e.Message}. At: {e.StackTrace}");
				throw;
			}
			return response;
		}

		public T CreateItem<T>() where T : BaseModel, new() {
			var entity = CreateEntity(typeof(T));
			return CreateItem<T>(entity);
		}

		public void DeleteItem<T>(T model) where T : BaseModel {
			if (!Items.ContainsKey(model.Id)) {
				// TODO #3.
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
