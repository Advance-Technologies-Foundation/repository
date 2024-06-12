namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using Terrasoft.Common;
	using Terrasoft.Nui.ServiceModel.DataContract;

	#region Class: ExpressionBuilderExtensions

	internal static class ExpressionBuilderExtensions
	{
		#region Class: DataPathItem

		internal class DataPathItem
		{
			public DataTable DataTable { get; set; }
			public DataColumn DataColumn { get; set; }
		}

		#endregion

		#region Class: DataPath

		internal class DataPath
		{
			public List<DataPathItem> Items { get; set; }

			public DataPath() {
				Items = new List<DataPathItem>();
			}
		}

		#endregion


		#region Methods: Private

		private static void ParsePath(this DataTable dataTable, string path, Action<DataTable, DataColumn> action) {
			var workTable = dataTable;
			path.Split('.').ForEach(pathPart => {
				var column = workTable.Columns[pathPart];
				action.Invoke(workTable, column);
				var lookupInfo = workTable.GetLookupRelationship(column.ColumnName);
				if (lookupInfo != null) {
					workTable = dataTable.DataSet.Tables[lookupInfo.ReferenceSchemaName];
				}
			});
		}

		#endregion

		#region Methods: Internal

		internal static DataPath GetSchemaColumnDataPath(this DataTable dataTable, string path) {
			var dataPath = new DataPath();
			dataTable.ParsePath(path, (table, dataColumn) => {
				dataPath.Items.Add(new DataPathItem() {
					DataTable = dataTable,
					DataColumn = dataColumn
				});
			});
			return dataPath;
		}

		internal static Type GetValueType(this Terrasoft.Nui.ServiceModel.DataContract.DataValueType dataValueType) {
			switch (dataValueType) {
				case DataValueType.Text:
				case DataValueType.ShortText:
				case DataValueType.MediumText:
				case DataValueType.LongText:
				case DataValueType.RichText:
					return typeof(string);
				case DataValueType.Boolean:
					return typeof(bool);
				case DataValueType.Float:
				case DataValueType.Float1:
				case DataValueType.Float2:
				case DataValueType.Float3:
				case DataValueType.Float4:
				case DataValueType.Float8:
					return typeof(decimal);
				case DataValueType.Integer:
					return typeof(int);
				case DataValueType.Lookup:
				case DataValueType.Enum:
				case DataValueType.Guid:
					return typeof(Guid);
				default:
					throw new NotImplementedException();
			}
		}

		internal static Type GetColumnPathDataType(this DataTable table, string columnPath)
		{
			var dataPath = table.GetSchemaColumnDataPath(columnPath);
			return dataPath.Items.Last().DataColumn.DataType;
		}

		internal static T GetTypedColumnValue<T>(this DataRow row, string columnName)
		{
			return row.IsNull(columnName) ? default(T) : row.Field<T>(columnName);
		}

		#endregion

		#region Methods: Public

		public static T GetTypedPathValue<T>(this DataRow row, string columnPath)
		{
			var dataPath = row.Table.GetSchemaColumnDataPath(columnPath);
			var activeRow = row;
			var hasValue = true;
			T value = default;
			dataPath.Items.ForEach(item => {
				if (!hasValue) {
					return;
				}
				if (item != dataPath.Items.Last()) {
					var lookupRelationship = item.DataTable.GetLookupRelationship(item.DataColumn.ColumnName);
					var lookupValue = activeRow.GetTypedColumnValue<Guid>(item.DataColumn.ColumnName);
					activeRow = activeRow.Table.DataSet.Tables[lookupRelationship.ReferenceSchemaName].AsEnumerable()
						.FirstOrDefault(x => x.GetTypedColumnValue<Guid>("Id") == lookupValue);
					if (lookupValue == Guid.Empty || activeRow == null) {
						hasValue = false;
					}
				} else {
					value = activeRow.GetTypedColumnValue<T>(item.DataColumn.ColumnName);
				}
			});
			return value;
		}

		public static bool PathHasValue(this DataRow row, string columnPath)
		{
			var dataPath = row.Table.GetSchemaColumnDataPath(columnPath);
			var activeRow = row;
			var hasValue = true;
			dataPath.Items.ForEach(item => {
				hasValue = hasValue && activeRow != null && !activeRow.IsNull(item.DataColumn);
				if (!hasValue) {
					return;
				}
				var lookupRelationship = item.DataTable.GetLookupRelationship(item.DataColumn.ColumnName);
				if (item == dataPath.Items.Last() || lookupRelationship == null ||
					item.DataColumn.DataType != typeof(Guid)) {
					return;
				}
				var lookupValue = activeRow.GetTypedColumnValue<Guid>(item.DataColumn.ColumnName);
				activeRow = activeRow.Table.DataSet.Tables[lookupRelationship.ReferenceSchemaName].AsEnumerable()
					.FirstOrDefault(x => x.GetTypedColumnValue<Guid>("Id") == lookupValue);
			});
			return hasValue;
		}

		#endregion

	}

	#endregion

}
