using System;
using System.Collections.Generic;
using System.Linq;
using ATF.Repository.ExpressionConverters;
using Terrasoft.Core.Entities;
using Terrasoft.Nui.ServiceModel.DataContract;
using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;

namespace ATF.Repository.Queryables
{
	internal class ModelQueryFilterBuilder
	{


		internal static Filter GenerateFilter(ExpressionMetadata filterMetadata) {
			switch (filterMetadata.NodeType) {
				case ExpressionMetadataNodeType.Comparison:
					return GenerateComparisonFilter(filterMetadata);
				case ExpressionMetadataNodeType.Group:
					return GenerateFilterGroup<Filter>(filterMetadata);
				case ExpressionMetadataNodeType.Column:
					return GenerateSingleColumnFilter(filterMetadata);
				default:
					throw new NotImplementedException();
			}
		}

		private static Filter GenerateSingleColumnFilter(ExpressionMetadata filterMetadata) {
			if (filterMetadata.Parameter?.Type == typeof(bool)) {
				return GenerateBooleanSingleColumnFilter(filterMetadata);
			}
			throw new NotImplementedException();
		}

		private static Filter GenerateBooleanSingleColumnFilter(ExpressionMetadata filterMetadata) {
			var left = ConvertExpression(filterMetadata);
			var right = ConvertToBaseExpression(DataValueType.Boolean, true);
			return new Filter() {
				FilterType = FilterType.CompareFilter,
				LeftExpression = left,
				ComparisonType = FilterComparisonType.Equal,
				RightExpression = right
			};
		}

		private static T GenerateFilterGroup<T>(ExpressionMetadata filterMetadata) where T: Filter, new() {
			var filters = new Dictionary<string, Filter>();
			filterMetadata.Items.ForEach(fm => {
				filters.Add(Guid.NewGuid().ToString(), GenerateFilter(fm));
			});
			return new T() {
				FilterType = FilterType.FilterGroup,
				LogicalOperation = filterMetadata.LogicalOperation,
				Items = filters
			};
		}

		private static Filter GenerateComparisonFilter(ExpressionMetadata filterMetadata) {
			var rightExpressions = filterMetadata.RightExpressions.Select(ConvertExpression).ToList();
			var filter = new Filter() {
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

		private static BaseExpression ConvertExpression(ExpressionMetadata filterMetadataLeftExpression) {
			return filterMetadataLeftExpression.NodeType == ExpressionMetadataNodeType.Column
				? ConvertToColumnExpression(filterMetadataLeftExpression)
				: ConvertToBaseExpression(filterMetadataLeftExpression);
		}

		private static ColumnExpression ConvertToColumnExpression(ExpressionMetadata filterMetadataLeftExpression) {
			return new ColumnExpression() {
				ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn,
				ColumnPath = filterMetadataLeftExpression.Parameter.ColumnPath
			};
		}

		private static BaseExpression ConvertToBaseExpression(ExpressionMetadata filterMetadataLeftExpression) {
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

		private static BaseExpression ConvertToBaseExpression(DataValueType type, object value) {
			return new BaseExpression() {
				ExpressionType = EntitySchemaQueryExpressionType.Parameter,
				Parameter = new Parameter() {
					Value = value,
					DataValueType = type
				}
			};
		}


	}
}
