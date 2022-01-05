namespace ATF.Repository.UnitTests.Utilities
{
	using System;
	using System.Linq;
	using ATF.Repository.Replicas;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;
	using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;
	using DataValueType = Terrasoft.Nui.ServiceModel.DataContract.DataValueType;

	public class QueryBuilderUtilities
	{
		public static ISelectQuery BuildSelectQuery(string schemaName, int rowCount = 100, int rowOffset = 0) {
			return new SelectQueryReplica() {
				RootSchemaName = schemaName,
				RowCount = rowCount,
				RowsOffset = rowOffset,
				Columns = new SelectQueryColumnsReplica(),
				Filters = new FilterGroupReplica()
			};
		}

		public static void AddColumn(ISelectQuery query, string columnPath, string alias = "", OrderDirection orderDirection = OrderDirection.None, int orderPosition = -1) {
			var key = string.IsNullOrEmpty(alias) ? columnPath : alias;
			var column = new SelectQueryColumnReplica() {
				Expression = new ColumnExpressionReplica() {
					ColumnPath = columnPath,
					ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn
				},
				OrderDirection = orderDirection,
				OrderPosition = orderPosition
			};
			query.Columns.Items.Add(key, column);
		}

		public static IFilter CreateComparisonFilter(string columnPath, FilterComparisonType comparisonType,
			DataValueType dataValueType, params object[] values) {
			var rightExpressions = values.Select(value => CreateParameterExpression(value, dataValueType)).ToList();
			var filter = new FilterReplica() {
				FilterType = FilterType.CompareFilter,
				ComparisonType = comparisonType,
				IsEnabled = true,
				LeftExpression = CreateColumnExpression(columnPath),
				RightExpression = rightExpressions.Count == 1 ? rightExpressions.First() : null,
				RightExpressions = rightExpressions.Count > 1 ? rightExpressions.ToArray() : null
			};
			if (comparisonType == FilterComparisonType.IsNull) {
				filter.IsNull = true;
				filter.FilterType = FilterType.IsNullFilter;
			}

			return filter;
		}

		private static BaseExpressionReplica CreateDefaultCreateParameterExpression(DataValueType dataValueType) {
			var type = DataValueTypeUtilities.ConvertDataValueTypeToType(dataValueType);
			var value = type.IsValueType
				? Activator.CreateInstance(type)
				: null;
			return CreateParameterExpression(value, dataValueType);
		}

		private static BaseExpressionReplica CreateParameterExpression(object value, DataValueType dataValueType) {
			return new BaseExpressionReplica() {
				ExpressionType = EntitySchemaQueryExpressionType.Parameter,
				Parameter = new ParameterReplica() {
					DataValueType = dataValueType,
					Value = value
				}
			};
		}

		private static ColumnExpressionReplica CreateColumnExpression(string columnPath) {
			return new ColumnExpressionReplica() {
				ColumnPath = columnPath,
				ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn
			};
		}


		public static IFilter CreateNullFilter(string columnPath, DataValueType dataValueType, FilterComparisonType comparisonType = FilterComparisonType.IsNull) {
			return CreateComparisonFilter(columnPath, comparisonType, dataValueType);
		}

		public static IFilter CreateFilterGroup(LogicalOperationStrict logicalOperation) {
			return new FilterGroupReplica() { LogicalOperation = logicalOperation};
		}
	}
}

