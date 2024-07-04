namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Linq.Expressions;
	using Terrasoft.Nui.ServiceModel.DataContract;

	#region Class: ExpressionBuilderExtensions

	internal static class ExpressionBuilderExtensions
	{

		#region Methods: Private

		private static DataRow GetLastActiveRow(this DataRow row, string path)
		{
			var schemaPath = row.Table.GetSchemaPath(path);
			var activeRow = row;
			var last = schemaPath.First.PathItems.LastOrDefault();
			schemaPath.First.PathItems.ForEach(pathItem => {
				if (activeRow == null || (pathItem == last && schemaPath.DetailPart == null)) {
					return;
				}
				var lookupValue = activeRow.GetTypedColumnValue<Guid>(pathItem.DataColumn.ColumnName);
				activeRow = lookupValue != Guid.Empty
					? pathItem.LookupTable.AsEnumerable()
						.FirstOrDefault(x =>
							x.GetTypedColumnValue<Guid>(DataStore.DefaultPrimaryValueColumnName) == lookupValue)
					: null;
			});
			return activeRow;
		}

		#endregion

		#region Methods: Internal

		internal static T GetTypedColumnValue<T>(this DataRow row, string columnName)
		{
			return row.IsNull(columnName) ? default(T) : row.Field<T>(columnName);
		}
		internal static SchemaPath GetSchemaPath(this DataTable dataTable, string path) {
			return PathParser.Parse(dataTable, path);
		}

		internal static Type GetSchemaPathDataType(this DataTable table, string path)
		{
			var schemaPath = table.GetSchemaPath(path);
			var columnPath = schemaPath.Last ?? schemaPath.First;
			return columnPath.PathItems.Last().DataColumn.DataType;
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
				case DataValueType.DateTime:
				case DataValueType.Date:
				case DataValueType.Time:
					return typeof(DateTime);
				default:
					throw new NotImplementedException();
			}
		}
		internal static List<DataRow> GetFilteredItems(this DataTable dataTable, Expression filter) {
			var filteredItems = new List<DataRow>();

			var filterExpression = (Expression<Func<DataRow, bool>>)filter;
			Func<DataRow, bool> filterMethod = filterExpression.Compile();

			foreach (var dataRow in dataTable.AsEnumerable()) {
				if (filterMethod.Invoke(dataRow)) {
					filteredItems.Add(dataRow);
				}
			}

			return filteredItems;
		}

		internal static List<DataRow> GetSortedItems(this List<DataRow> items, ExpressionContext expressionContext, Expression sortExpression) {
			if (!items.Any()) {
				return items;
			}

			if (sortExpression == null) {
				return items;
			}

			var exp = Expression.Lambda(sortExpression, expressionContext.RowsExpression);
			var sortLambdaExpression = (Expression<Func<List<DataRow>, IOrderedEnumerable<DataRow>>>)exp;
			var sortMethod = sortLambdaExpression.Compile();
			var response = sortMethod.Invoke(items).ToList();
			return response;
		}

		internal static List<DataRow> TakeItems(this List<DataRow> items, int takeCount) {
			return takeCount < 0 ? items : items.Take(takeCount).ToList();
		}

		internal static List<DataRow> SkipItems(this List<DataRow> items, int skipCount) {
			return skipCount <= 0 ? items : items.Skip(skipCount).ToList();
		}

		#endregion

		#region Methods: Public

		public static bool HasTypedPathValue(this DataRow row, string path)
		{
			var schemaPath = row.Table.GetSchemaPath(path);
			var activeRow = row.GetLastActiveRow(path);
			var last = schemaPath.First.PathItems.LastOrDefault();
			return activeRow != null && (schemaPath.DetailPart != null || !activeRow.IsNull(last.DataColumn));
		}

		public static T GetTypedPathValue<T>(this DataRow row, string path)
		{
			T value = default;
			if (!row.HasTypedPathValue(path)) {
				return value;
			}
			var activeRow = row.GetLastActiveRow(path);
			if (activeRow == null) {
				return value;
			}
			var schemaPath = row.Table.GetSchemaPath(path);
			var last = schemaPath.First.PathItems.Last();
			return activeRow.GetTypedColumnValue<T>(last.DataColumn.ColumnName);
		}

		public static List<DataRow> Detail(this DataRow row, string path) {
			var response = new List<DataRow>();
			if (!row.HasTypedPathValue(path)) {
				return response;
			}
			var schemaPath = row.Table.GetSchemaPath(path);
			if (schemaPath?.DetailPart == null) {
				return response;
			}
			var activeRow = row.GetLastActiveRow(path);
			if (activeRow == null) {
				return response;
			}

			var masterValue = activeRow.GetTypedColumnValue<Guid>(schemaPath.DetailPart.MasterDataColumn.ColumnName);
			if (masterValue == Guid.Empty || schemaPath.DetailPart.DetailDataColumn.DataType != typeof(Guid)) {
				return response;
			}
			var detailDataColumnName = schemaPath.DetailPart.DetailDataColumn.ColumnName;
			return schemaPath.DetailPart.DetailDataTable.AsEnumerable()
				.Where(x => x.GetTypedColumnValue<Guid>(detailDataColumnName) == masterValue).ToList();
		}

		#endregion

	}

	#endregion

}
