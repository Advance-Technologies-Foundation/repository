﻿using Terrasoft.Nui.ServiceModel.DataContract;

namespace ATF.Repository.UnitTests.Utilities
{
	using System;
	using System.Linq;
	using ATF.Repository.Mapping;
	using ATF.Repository.Replicas;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;
	using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;
	using DataValueType = Terrasoft.Nui.ServiceModel.DataContract.DataValueType;

	public static class TestSelectBuilder
	{
		public static ISelectQuery GetTestSelectQuery<T>(Action<IFilterGroup> enrichFilter = null) where T: BaseModel {
			var modelType = typeof(T);
			var schemaName = ModelUtilities.GetSchemaName(modelType);
			var columns = new SelectQueryColumnsReplica();
			ModelMapper.GetModelItems(modelType).Where(modelItem =>
					modelItem.PropertyType == ModelItemType.Column || modelItem.PropertyType == ModelItemType.Lookup)
				.ForEach(property => {
					if (!columns.Items.ContainsKey(property.EntityColumnName)) {
						columns.Items.Add(property.EntityColumnName, new SelectQueryColumnReplica() {
							Expression = new ColumnExpressionReplica() {
								ColumnPath = property.EntityColumnName,
								ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn
							},
							OrderDirection = OrderDirection.None,
							OrderPosition = -1
						});
					}
				});
			var filters = new FilterGroupReplica();
			enrichFilter?.Invoke(filters);

			return new SelectQueryReplica() {
				RootSchemaName = schemaName,
				AllColumns = false,
				IsDistinct = false,
				RowCount = 20000,
				Columns = columns,
				Filters = filters
			};
		}

		public static IFilter CreateComparisonFilter(string columnPath, FilterComparisonType comparisonType,
			DataValueType dataValueType, params object[] values) {
			var rightExpressions = values.Select(value => new BaseExpressionReplica() {
				ExpressionType = EntitySchemaQueryExpressionType.Parameter,
				Parameter = new ParameterReplica() {
					Value = GetQueryValue(value, dataValueType),
					DataValueType = dataValueType
				}
			}).ToList();
			return new FilterReplica() {
				FilterType = rightExpressions.Count > 1 ? FilterType.InFilter : FilterType.CompareFilter,
				ComparisonType = comparisonType,
				IsEnabled = true,
				LeftExpression = new ColumnExpressionReplica() {
					ColumnPath = columnPath,
					ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn
				},
				RightExpression = rightExpressions.Count == 1 ? rightExpressions.First() : null,
				RightExpressions = rightExpressions.Count > 1 ? rightExpressions.ToArray() : null
			};
		}

		public static IFilter CreateIsNullFilter(string columnPath, bool isNull,
			DataValueType dataValueType) {
			return new FilterReplica() {
				FilterType = FilterType.IsNullFilter,
				ComparisonType = isNull ? FilterComparisonType.IsNull : FilterComparisonType.IsNotNull,
				IsEnabled = true,
				LeftExpression = new ColumnExpressionReplica() {
					ColumnPath = columnPath,
					ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn
				}
			};
		}

		public static IFilter CreateDatePartFilter(FilterComparisonType comparisonType, string columnPath, DatePart datePart, object value) {
			return new FilterReplica() {
				FilterType = FilterType.CompareFilter,
				ComparisonType = comparisonType,
				TrimDateTimeParameterToDate = false,
				IsEnabled = true,
				LeftExpression = new ColumnExpressionReplica() {
					ExpressionType = EntitySchemaQueryExpressionType.Function,
					FunctionType = FunctionType.DatePart,
					FunctionArgument = new FunctionArgumentReplica() {
						ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn,
						ColumnPath = columnPath
					},
					DatePartType = datePart
				},
				RightExpression = new BaseExpressionReplica() {
					ExpressionType = EntitySchemaQueryExpressionType.Parameter,
					Parameter = new ParameterReplica() {
						DataValueType = DataValueType.Integer,
						Value = value
					}
				}
			};
		}

		public static object GetQueryValue(object rawValue, DataValueType dataValueType) {
			if (DataValueTypeUtilities.IsDateDataValueType(dataValueType) && rawValue != null) {
				return $"\"{((DateTime)rawValue):yyyy-MM-ddTHH:mm:ss.fff}\"";
			}

			return rawValue;
		}

		public static IFilter CreateFilterGroup(LogicalOperationStrict logicalOperation) {
			return new FilterGroupReplica() {
				LogicalOperation = logicalOperation
			};
		}
	}
}
