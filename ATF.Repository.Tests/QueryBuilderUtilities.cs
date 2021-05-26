using System;
using System.Collections.Generic;
using System.Linq;
using Terrasoft.Common;
using Terrasoft.Core.Entities;
using Terrasoft.Nui.ServiceModel.DataContract;
using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;

namespace ATF.Repository.Tests
{
	public class QueryBuilderUtilities
	{
		public static SelectQuery BuildSelectQuery(string schemaName, int rowCount = 100, int rowOffset = 0) {
			return new SelectQuery() {
				RootSchemaName = schemaName,
				RowCount = rowCount,
				RowsOffset = rowOffset,
				Columns = new SelectQueryColumns() {Items = new Dictionary<string, SelectQueryColumn>()},
				Filters = new Filters() {
					FilterType = FilterType.FilterGroup,
					Items = new Dictionary<string, Filter>()
				}
			};
		}

		public static void AddColumn(SelectQuery query, string columnPath, string alias = "", OrderDirection orderDirection = OrderDirection.None, int orderPosition = -1) {
			var key = string.IsNullOrEmpty(alias) ? columnPath : alias;
			var column = new SelectQueryColumn() {
				Expression = new ColumnExpression() {
					ColumnPath = columnPath,
					ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn
				},
				OrderDirection = orderDirection,
				OrderPosition = orderPosition
			};
			query.Columns.Items.Add(key, column);
		}

		public static Filter CreateComparisonFilter(string columnPath, FilterComparisonType comparisonType,
			DataValueType dataValueType, params object[] values) {
			var rightExpressions = values.Select(value => CreateParameterExpression(value, dataValueType)).ToList();
			var filter = new Filter() {
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

		private static BaseExpression CreateDefaultCreateParameterExpression(DataValueType dataValueType) {
			var type = DataValueTypeUtilities.ConvertDataValueTypeToType(dataValueType);
			var value = type.IsValueType
				? Activator.CreateInstance(type)
				: null;
			return CreateParameterExpression(value, dataValueType);
		}

		private static BaseExpression CreateParameterExpression(object value, DataValueType dataValueType) {
			return new BaseExpression() {
				ExpressionType = EntitySchemaQueryExpressionType.Parameter,
				Parameter = new Parameter() {
					DataValueType = dataValueType,
					Value = value
				}
			};
		}

		private static ColumnExpression CreateColumnExpression(string columnPath) {
			return new ColumnExpression() {
				ColumnPath = columnPath,
				ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn
			};
		}


		public static Filter CreateNullFilter(string columnPath, DataValueType dataValueType, FilterComparisonType comparisonType = FilterComparisonType.IsNull) {
			return CreateComparisonFilter(columnPath, comparisonType, dataValueType);
		}

		public static Filter CreateFilterGroup(LogicalOperationStrict logicalOperation) {
			return new Filter() {
				FilterType = FilterType.FilterGroup,
				LogicalOperation = logicalOperation,
				Items = new Dictionary<string, Filter>()
			};
		}
	}
}
