using System;
using System.Collections.Generic;
using System.Linq;
using ATF.Repository.Mapping;
using Terrasoft.Common;
using Terrasoft.Core.Entities;
using Terrasoft.Nui.ServiceModel.DataContract;
using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;

namespace ATF.Repository.UnitTests.Utilities
{
	public static class TestSelectBuilder
	{
		public static SelectQuery GetTestSelectQuery<T>(Action<Filters> enrichFilter = null) where T: BaseModel {
			var modelType = typeof(T);
			var schemaName = ModelUtilities.GetSchemaName(modelType);
			var columns = new SelectQueryColumns() {Items = new Dictionary<string, SelectQueryColumn>()};
			ModelMapper.GetModelItems(modelType).Where(modelItem =>
					modelItem.PropertyType == ModelItemType.Column || modelItem.PropertyType == ModelItemType.Lookup)
				.ForEach(property => {
					if (!columns.Items.ContainsKey(property.EntityColumnName)) {
						columns.Items.Add(property.EntityColumnName, new SelectQueryColumn() {
							Expression = new ColumnExpression() {
								ColumnPath = property.EntityColumnName,
								ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn
							},
							OrderDirection = OrderDirection.None,
							OrderPosition = -1
						});
					}
				});
			var filters = new Filters() {
				FilterType = FilterType.FilterGroup,
				LogicalOperation = LogicalOperationStrict.And,
				Items = new Dictionary<string, Filter>()
			};
			enrichFilter?.Invoke(filters);

			return new SelectQuery() {
				RootSchemaName = schemaName,
				AllColumns = false,
				IsDistinct = false,
				RowCount = 100,
				Columns = columns,
				Filters = filters
			};
		}

		public static Filter CreateComparisonFilter(string columnPath, FilterComparisonType comparisonType,
			DataValueType dataValueType, params object[] values) {
			var rightExpressions = values.Select(value => new BaseExpression() {
				ExpressionType = EntitySchemaQueryExpressionType.Parameter,
				Parameter = new Parameter() {
					Value = GetQueryValue(value, dataValueType),
					DataValueType = dataValueType
				}
			}).ToList();
			return new Filter() {
				FilterType = rightExpressions.Count > 1 ? FilterType.InFilter : FilterType.CompareFilter,
				ComparisonType = comparisonType,
				IsEnabled = true,
				LeftExpression = new ColumnExpression() {
					ColumnPath = columnPath,
					ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn
				},
				RightExpression = rightExpressions.Count == 1 ? rightExpressions.First() : null,
				RightExpressions = rightExpressions.Count > 1 ? rightExpressions.ToArray() : null
			};
		}

		public static object GetQueryValue(object rawValue, DataValueType dataValueType) {
			if (DataValueTypeUtilities.IsDateDataValueType(dataValueType) && rawValue != null) {
				return $"\"{((DateTime)rawValue):yyyy-MM-ddTHH:mm:ss.fff}\"";
			}

			return rawValue;
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
