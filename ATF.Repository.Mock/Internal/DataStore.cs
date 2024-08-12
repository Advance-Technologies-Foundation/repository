namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.Linq;
	using ATF.Repository.Mapping;
	using Terrasoft.Common;

	#region Class: DataStore

	internal class DataStore: IDataStore
	{
		#region Fields: Private

		private readonly DataSet _dataSet;
		private readonly List<Type> _registeredModelTypes;

		private readonly List<string> _defaultColumnNames = new List<string>()
			{ "Id", "CreatedOn", "CreatedBy", "ModifiedOn", "ModifiedBy", "ProcessListeners" };

		#endregion

		#region Fields: Internal

		internal const string DefaultPrimaryValueColumnName = "Id";

		#endregion

		#region Properties: Public

		public bool EmulateSystemColumnsBehavior { get; set; }

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

		private void SetModelDataToDefaultValues<T>(T model) where T : BaseModel, new() {
			var schemaName = GetRegisteredSchemaName(model.GetType());
			var table = FetchDataTable(schemaName);
			ModelMapper.GetProperties(typeof(T)).ForEach(propertyItem => {
				if (EmulateSystemColumnsBehavior && IsSystemColumn(propertyItem.EntityColumnName)) {
					return;
				}
				var column = table.Columns.Contains(propertyItem.EntityColumnName)
					? table.Columns[propertyItem.EntityColumnName]
					: null;
				if (column == null) {
					return;
				}
				var propertyValue = propertyItem.PropertyInfo.GetValue(model);
				if (IsNotDefaultValue(propertyItem.PropertyInfo.PropertyType, propertyValue)) {
					column.DefaultValue = GetTypedValue(propertyItem.PropertyInfo.PropertyType, propertyValue);
				}
			});
		}

		private bool IsSystemColumn(string columnName) {
			return _defaultColumnNames.Contains(columnName);
		}

		private bool IsNotDefaultValue(Type type, object value) {
			var method = RepositoryReflectionUtilities.GetGenericMethod(GetType(), "IsNotDefaultValue", type);
			return (bool)method.Invoke(this, new object[] { value });
		}

		private bool IsNotDefaultValue<T>(object value) {
			var def = default(T);
			if (value == null) {
				return false;
			}
			return def != null && value is T typedValue && def.Equals(typedValue);
		}


		private void SetRawDataToDataSet(string schemaName, Dictionary<string, object> values) {
			var dataTable = FetchDataTable(schemaName);
			var row = dataTable.NewRow();
			SetValueToDataRow(row, values);
			dataTable.Rows.Add(row);
		}

		private void SetValueToDataRow(DataRow dataRow, Dictionary<string, object> values) {
			values.ForEach(item => {
				var column = FindDataColumn(dataRow.Table, item.Key);
				if (column == null) {
					return;
				}
				var value = GetTypedValue(column.DataType, item.Value);
				dataRow[column.ColumnName] = value;
			});
		}

		private DataColumn FindDataColumn(DataTable dataTable, string columnName) {
			if (dataTable.Columns.Contains(columnName)) {
				return dataTable.Columns[columnName];
			}

			DataColumn column = null;
			foreach (DataColumn dataTableColumn in dataTable.Columns) {
				if (column != null) {
					continue;
				}
				if (dataTableColumn.DataType == typeof(Guid) && $"{dataTableColumn.ColumnName}Id" == columnName) {
					column = dataTableColumn;
				}
			}

			return column;
		}

		private object GetTypedValue(Type dataType, object value) {
			var method = RepositoryReflectionUtilities.GetGenericMethod(GetType(), "GetTypedValue", dataType);
			return method.Invoke(this, new object[] { value });
		}

		private T GetTypedValue<T>(object value) {
			if (typeof(T) == typeof(DateTime) && value is DateTime dateTimeValue) {
				value = dateTimeValue.TrimMilliseconds();
			}

			if (typeof(T) == typeof(Guid) && value is string stringValue && Guid.TryParse(stringValue, out var guidValue)) {
				value = guidValue;
			}
			if (typeof(T) == typeof(Guid) && value == null) {
				value = Guid.Empty;
			}
			return (T)Convert.ChangeType(value, typeof(T));
		}

		private Dictionary<string, object> GetActualInsertingValues(Dictionary<string, object> values) {
			if (!values.ContainsKey(DefaultPrimaryValueColumnName)) {
					values.Add(DefaultPrimaryValueColumnName, Guid.NewGuid());
			}
			if (!EmulateSystemColumnsBehavior) {
				return values;
			}
			if (values.ContainsKey("CreatedOn")) {
				values["CreatedOn"] = DateTime.Now.TrimMilliseconds();
			}
			if (values.ContainsKey("ModifiedOn")) {
				values["ModifiedOn"] = DateTime.Now.TrimMilliseconds();
			}
			return values;
		}

		private Dictionary<string, object> GetActualUpdatingValues(Dictionary<string, object> values) {
			if (!EmulateSystemColumnsBehavior) {
				return values.Where(x => x.Key != DefaultPrimaryValueColumnName).ToDictionary(x => x.Key, x => x.Value);
			}
			var actualValues = values.Where(x => !IsSystemColumn(x.Key)).ToDictionary(x => x.Key, x => x.Value);
			if (values.ContainsKey("ModifiedOn")) {
				actualValues["ModifiedOn"] = DateTime.Now.TrimMilliseconds();
			}
			return actualValues;
		}

		#endregion

		#region Methods: Internal

		internal void InsertRecord(string schemaName, Dictionary<string, object> values) {
			var actualValues = GetActualInsertingValues(values);
			AddModelRawData(schemaName, new List<Dictionary<string, object>>() {actualValues});
		}

		internal void UpdateRecords(List<DataRow> rows, Dictionary<string, object> values) {
			var actualValues = GetActualUpdatingValues(values);
			rows.ForEach(row=> SetValueToDataRow(row, actualValues));
		}

		internal void DeleteRecords(List<DataRow> rows) {
			rows.ForEach(row => row.Delete());
		}

		internal Dictionary<string, object> GetDefaultValues(string schemaName) {
			var response = new Dictionary<string, object>();
			var dataTable = FetchDataTable(schemaName);
			foreach (DataColumn column in dataTable.Columns) {
				if (IsNotDefaultValue(column.DataType, column.DefaultValue)) {
					response.Add(column.ColumnName, column.DefaultValue);
				}
			}
			return response;
		}

		private void ParseFile(string filePath) {
			if (DataFileParser.TryParse(filePath, out var dataFileDto)) {
				AddModelRawData(dataFileDto.SchemaName, dataFileDto.Records);
			}
		}

		#endregion

		#region Methods: Public

		public void RegisterModelSchema<T>() where T : BaseModel {
			RegisterModelSchema(typeof(T));
		}

		public void RegisterModelSchema(params Type[] types) {
			types.ForEach(RegisterModelSchemaInDataStore);
		}

		public void SetDefaultValues<T>(Action<T> action) where T : BaseModel, new() {
			var model = new T {
				Id = Guid.NewGuid()
			};
			action.Invoke(model);
			SetModelDataToDefaultValues(model);
		}

		public T AddModel<T>(Action<T> action) where T : BaseModel, new() {
			return AddModel<T>(Guid.NewGuid(), action);
		}

		public T AddModel<T>(Guid recordId, Action<T> action) where T : BaseModel, new() {
			RegisterModelSchema(typeof(T));
			var model = new T {
				Id = recordId
			};
			action.Invoke(model);
			SetModelDataToDataSet(model);
			return model;
		}


		public void AddModelRawData<T>(List<Dictionary<string, object>> recordList) where T : BaseModel {
			RegisterModelSchema(typeof(T));
			var schemaName = GetRegisteredSchemaName(typeof(T));
			AddModelRawData(schemaName, recordList);
		}

		public void AddModelRawData(string schemaName, List<Dictionary<string, object>> recordList) {
			recordList.ForEach(recordValues => SetRawDataToDataSet(schemaName, recordValues));
		}

		public void LoadDataFromFileStore(string folderPath) {
			var files = Directory.GetFiles(folderPath);
			files.ForEach(ParseFile);
		}

		#endregion

	}

	#endregion

}
