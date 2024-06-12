namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using ATF.Repository.Mapping;
	using Terrasoft.Common;

	#region Class: DataStore

	internal class DataStore: IDataStore
	{
		#region Fields: Private

		private readonly DataSet _dataSet;
		private readonly List<Type> _registeredModelTypes;
		private const string DefaultPrimaryValueColumnName = "Id";

		#endregion

		#region Constructors: Internal

		internal DataStore(DataSet dataSet) {
			_dataSet = dataSet;
			_registeredModelTypes = new List<Type>();
		}

		#endregion

		#region Methods: Private

		private void RegisterModelSchemaInDataStore(Type type) {
			if (_registeredModelTypes.Contains(type)) {
				return;
			}
			_registeredModelTypes.Add(type);
			var schemaName = ModelUtilities.GetSchemaName(type);
			if (string.IsNullOrEmpty(schemaName)) {
				throw new Exception(schemaName);
			}

			var dataTable = FetchDataTable(schemaName);
			var modelItems = ModelMapper.GetModelItems(type);

			ApplyModelItemsOnTable(dataTable, modelItems);
		}

		private void ApplyModelItemsOnTable(DataTable dataTable, List<ModelItem> modelItems) {
			foreach (var modelItem in modelItems.Where(x=>x.PropertyType == ModelItemType.Column)) {
				ApplySchemaPropertyOnTable(dataTable, modelItem);
			}
			foreach (var modelItem in modelItems.Where(x=>x.PropertyType == ModelItemType.Lookup)) {
				ApplySchemaPropertyOnTable(dataTable, modelItem.EntityColumnName, typeof(Guid));
				RegisterModelSchemaInDataStore(modelItem.DataValueType);
				RegisterLookupRelationship(dataTable, modelItem);
			}
			foreach (var modelItem in modelItems.Where(x=>x.PropertyType == ModelItemType.Detail)) {
				RegisterModelSchemaInDataStore(modelItem.DataValueType);
				RegisterDetailRelationship(dataTable, modelItem);
			}
		}

		private void RegisterDetailRelationship(DataTable dataTable, ModelItem modelItem) {
			var detailSchemaName = GetRegisteredSchemaName(modelItem.DataValueType);
			var detailColumnName = GetSchemaColumnName(modelItem.DataValueType, modelItem.DetailLinkPropertyName);
			var masterColumnName = !string.IsNullOrEmpty(modelItem.MasterEntityColumnName) && modelItem.MasterEntityColumnName != DefaultPrimaryValueColumnName
				? GetSchemaColumnName(modelItem.DataValueType, modelItem.DetailLinkPropertyName)
				: DefaultPrimaryValueColumnName;
			dataTable.RegisterDetailRelationship(modelItem.PropertyName, detailSchemaName, detailColumnName, masterColumnName);
		}

		private string GetSchemaColumnName(Type modelType, string modelPropertyName) {
			var schemaName = GetRegisteredSchemaName(modelType);
			var dataTable = FetchDataTable(schemaName);
			var properties = ModelMapper.GetProperties(modelType);
			var modelProperty = properties.FirstOrDefault(x =>
				x.PropertyName == modelPropertyName && x.DataValueType == typeof(Guid));
			if (modelProperty != null) {
				return modelProperty.EntityColumnName;
			}

			if (dataTable.Columns.Contains(modelPropertyName) &&
				dataTable.Columns[modelPropertyName].DataType == typeof(Guid)) {
				return modelPropertyName;
			}

			throw new Exception($"{schemaName} find {modelPropertyName}");
		}

		private void RegisterLookupRelationship(DataTable dataTable, ModelItem modelItem) {
			var lookupSchemaName = GetRegisteredSchemaName(modelItem.DataValueType);
			dataTable.RegisterLookupRelationship(modelItem.EntityColumnName, lookupSchemaName);
		}
		private string GetRegisteredSchemaName(Type type) {
			var lookupSchemaName = ModelUtilities.GetSchemaName(type);
			if (string.IsNullOrEmpty(lookupSchemaName) || !_dataSet.Tables.Contains(lookupSchemaName) ||
				!_dataSet.Tables[lookupSchemaName].Columns.Contains(DefaultPrimaryValueColumnName)) {
				throw new Exception(lookupSchemaName);
			}

			return lookupSchemaName;
		}

		private void ApplySchemaPropertyOnTable(DataTable dataTable, ModelItem modelItem) {
			ApplySchemaPropertyOnTable(dataTable, modelItem.EntityColumnName, modelItem.DataValueType);
		}

		private void ApplySchemaPropertyOnTable(DataTable dataTable, string columnName, Type dataValueType) {
			if (dataTable.Columns.Contains(columnName)) {
				var column = dataTable.Columns[columnName];
				if (column.DataType != dataValueType) {
					throw new Exception(
						$"{dataTable.TableName}.{columnName} type: {column.DataType.Name} != {dataValueType.Name}");
				}
			} else {
				dataTable.Columns.Add(columnName, dataValueType);
			}
		}

		private DataTable FetchDataTable(string schemaName) {
			if (!_dataSet.Tables.Contains(schemaName)) {
				_dataSet.Tables.Add(new DataTable(schemaName));
			}

			return _dataSet.Tables[schemaName];
		}

		private void SetModelDataToDataSet<T>(T model) where T : BaseModel, new() {
			var schemaName = GetRegisteredSchemaName(model.GetType());
			var values = new Dictionary<string, object>();
			ModelMapper.GetProperties(typeof(T)).ForEach(propertyItem => {
				var propertyValue = propertyItem.PropertyInfo.GetValue(model);
				values.Add(propertyItem.EntityColumnName, propertyValue);
			});
			SetRawDataToDataSet(schemaName, values);
		}

		private void SetRawDataToDataSet(string schemaName, Dictionary<string, object> values) {
			var dataTable = FetchDataTable(schemaName);
			var row = dataTable.NewRow();
			values.ForEach(item => {
				if (!dataTable.Columns.Contains(item.Key)) {
					return;
				}
				var value = GetTypedValue(dataTable.Columns[item.Key].DataType, item.Value);
				row[item.Key] = value;
			});
			dataTable.Rows.Add(row);
		}

		private object GetTypedValue(Type dataType, object value) {
			var method = RepositoryReflectionUtilities.GetGenericMethod(GetType(), "GetTypedValue", dataType);
			return method.Invoke(this, new object[] { value });
		}

		private T GetTypedValue<T>(object value) {
			return (T)Convert.ChangeType(value, typeof(T));
		}

		#endregion

		#region Methods: Public

		public void RegisterModelSchema<T>() where T : BaseModel {
			RegisterModelSchema(typeof(T));
		}

		public void RegisterModelSchema(params Type[] types) {
			types.ForEach(RegisterModelSchemaInDataStore);
		}

		public T AddModel<T>(Action<T> action) where T : BaseModel, new() {
			return AddModel<T>(Guid.NewGuid(), action);
		}

		public T AddModel<T>(Guid recordId, Action<T> action) where T : BaseModel, new() {
			var model = new T();
			model.Id = recordId;
			action.Invoke(model);
			SetModelDataToDataSet(model);
			return model;
		}


		public void AddModelRawData<T>(List<Dictionary<string, object>> recordList) where T : BaseModel {
			throw new NotImplementedException();
		}

		public void AddModelRawData(string schemaName, List<Dictionary<string, object>> recordList) {
			throw new NotImplementedException();
		}

		#endregion

	}

	#endregion

}
