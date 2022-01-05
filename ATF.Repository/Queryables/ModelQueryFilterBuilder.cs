namespace ATF.Repository.Queryables
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.Exceptions;
	using ATF.Repository.ExpressionConverters;
	using ATF.Repository.Replicas;
	using Terrasoft.Core.Entities;

	using DataValueType = Terrasoft.Nui.ServiceModel.DataContract.DataValueType;
	using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;

	internal static class ModelQueryFilterBuilder
	{
		internal static FilterReplica GenerateFilter(ExpressionMetadata filterMetadata) {
			switch (filterMetadata.NodeType) {
				case ExpressionMetadataNodeType.Comparison:
					return GenerateComparisonFilter(filterMetadata);
				case ExpressionMetadataNodeType.Group:
					return GenerateFilterGroup<FilterReplica>(filterMetadata);
				case ExpressionMetadataNodeType.Column:
					return GenerateSingleColumnFilter(filterMetadata);
				default:
					throw new NotImplementedException();
			}
		}

		private static FilterReplica GenerateSingleColumnFilter(ExpressionMetadata filterMetadata) {
			if (filterMetadata.Parameter?.Type == typeof(bool)) {
				return GenerateBooleanSingleColumnFilter(filterMetadata);
			}
			throw new NotImplementedException();
		}

		private static FilterReplica GenerateBooleanSingleColumnFilter(ExpressionMetadata filterMetadata) {
			var left = ConvertExpression(filterMetadata);
			var right = ConvertToBaseExpression(DataValueType.Boolean, true);
			return new FilterReplica() {
				FilterType = FilterType.CompareFilter,
				LeftExpression = left,
				ComparisonType = FilterComparisonType.Equal,
				RightExpression = right
			};
		}

		private static T GenerateFilterGroup<T>(ExpressionMetadata filterMetadata) where T: FilterReplica, new() {
			var filters = new Dictionary<string, IFilter>();
			filterMetadata.Items.ForEach(fm => {
				filters.Add(Guid.NewGuid().ToString(), GenerateFilter(fm));
			});
			return new T() {
				FilterType = FilterType.FilterGroup,
				LogicalOperation = filterMetadata.LogicalOperation,
				Items = filters
			};
		}

		private static FilterReplica GenerateComparisonFilter(ExpressionMetadata filterMetadata) {
			if (filterMetadata.LeftExpression?.NodeType == ExpressionMetadataNodeType.Detail) {
				return GenerateDetailComparisonFilter(filterMetadata);
			}

			return GenerateSimpleComparisonFilter(filterMetadata);
		}

		private static FilterReplica GenerateDetailComparisonFilter(ExpressionMetadata filterMetadata) {
			if (filterMetadata.LeftExpression.MethodName == ConvertableExpressionMethod.Any) {
				return GenerateExistsDetailFilter(filterMetadata);
			}

			return GenerateSimpleComparisonFilter(filterMetadata);
		}

		private static FilterReplica GenerateExistsDetailFilter(ExpressionMetadata filterMetadata) {
			var detailSelect = ModelQueryBuilder.BuildSelectQuery(filterMetadata.LeftExpression.DetailChain);
			return new FilterReplica() {
				FilterType = FilterType.Exists,
				ComparisonType = GetExistsDetailFilterComparisonType(filterMetadata),
				LeftExpression = new ColumnExpressionReplica() {
					ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn,
					ColumnPath = $"{filterMetadata.LeftExpression.Parameter.ColumnPath}.Id",
				},
				SubFilters = detailSelect.Filters
			};
		}

		private static FilterComparisonType GetExistsDetailFilterComparisonType(ExpressionMetadata filterMetadata) {
			return (filterMetadata.ComparisonType == FilterComparisonType.Equal) == (bool)filterMetadata.RightExpression.Parameter.Value
				? FilterComparisonType.Exists
				: FilterComparisonType.NotExists;
		}

		private static FilterReplica GenerateSimpleComparisonFilter(ExpressionMetadata filterMetadata) {
			var rightExpressions = filterMetadata.RightExpressions.Select(ConvertExpression).ToList();
			var filter = new FilterReplica() {
				FilterType = rightExpressions.Count > 1 ? FilterType.InFilter : FilterType.CompareFilter,
				LeftExpression = ConvertExpression(filterMetadata.LeftExpression),
				ComparisonType = filterMetadata.ComparisonType,
				IsNull = filterMetadata.ComparisonType == FilterComparisonType.IsNull,
				RightExpression = rightExpressions.Count == 1 ? rightExpressions.First() : null,
				RightExpressions = rightExpressions.Count > 1 ? rightExpressions.ToArray() : null
			};
			if (filterMetadata.ComparisonType == FilterComparisonType.IsNull || filterMetadata.ComparisonType == FilterComparisonType.IsNotNull) {
				filter.FilterType = FilterType.IsNullFilter;
			}
			return filter;
		}

		private static BaseExpressionReplica ConvertExpression(ExpressionMetadata filterMetadataLeftExpression) {
			switch (filterMetadataLeftExpression.NodeType) {
				case ExpressionMetadataNodeType.Column:
					return ConvertToColumnExpression(filterMetadataLeftExpression);
				case ExpressionMetadataNodeType.Detail:
					return ConvertToDetailExpression(filterMetadataLeftExpression);
				default:
					return ConvertToBaseExpression(filterMetadataLeftExpression);
			}
		}

		private static ColumnExpressionReplica ConvertToDetailExpression(ExpressionMetadata detailMetadataLeftExpression) {
			var detailSelect = ModelQueryBuilder.BuildSelectQuery(detailMetadataLeftExpression.DetailChain);
			if (detailSelect.Columns.Items.Count != 1) {
				throw new ExpressionApplierException();
			}
			var detailColumnExpression = detailSelect.Columns.Items.First().Value.Expression;
			var columnExpression = new ColumnExpressionReplica() {
				ExpressionType = EntitySchemaQueryExpressionType.SubQuery,
				FunctionType = detailColumnExpression.FunctionType,
				AggregationType = detailColumnExpression.AggregationType,
				ColumnPath = $"{detailMetadataLeftExpression.Parameter.ColumnPath}.{detailColumnExpression?.FunctionArgument?.ColumnPath ?? "Id"}",
				SubFilters = detailSelect.Filters
			};


			return columnExpression;
		}

		private static ColumnExpressionReplica ConvertToColumnExpression(ExpressionMetadata filterMetadataLeftExpression) {
			return new ColumnExpressionReplica() {
				ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn,
				ColumnPath = filterMetadataLeftExpression.Parameter.ColumnPath
			};
		}

		private static BaseExpressionReplica ConvertToBaseExpression(ExpressionMetadata filterMetadataLeftExpression) {
			var dataValueType = DataValueTypeUtilities.ConvertTypeToDataValueType(filterMetadataLeftExpression.Parameter.Type);
			var value = GetQueryValue(filterMetadataLeftExpression.Parameter.Value, dataValueType);
			return ConvertToBaseExpression(dataValueType, value);
		}

		private static object GetQueryValue(object rawValue, DataValueType dataValueType) {
			if (DataValueTypeUtilities.IsDateDataValueType(dataValueType) && rawValue != null) {
				return $"\"{((DateTime)rawValue):yyyy-MM-ddTHH:mm:ss.fff}\"";
			}

			return rawValue;
		}

		private static BaseExpressionReplica ConvertToBaseExpression(DataValueType type, object value) {
			return new BaseExpressionReplica() {
				ExpressionType = EntitySchemaQueryExpressionType.Parameter,
				Parameter = new ParameterReplica() {
					Value = value,
					DataValueType = type
				}
			};
		}


	}
}
