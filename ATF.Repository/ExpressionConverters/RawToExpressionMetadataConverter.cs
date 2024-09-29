using Terrasoft.Nui.ServiceModel.DataContract;

namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Linq.Expressions;
	using ATF.Repository.Exceptions;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;

	internal static class RawToExpressionMetadataConverter
	{
		internal static ExpressionMetadata Convert(RawExpressionMetadata rawExpressionMetadata,
			ExpressionModelMetadata modelMetadata) {
			if (ExpressionConverterUtilities.IsLogicalOperationExpressionType(rawExpressionMetadata.Type)) {
				return ConvertFilterGroup(rawExpressionMetadata, modelMetadata);
			}
			if (ExpressionConverterUtilities.IsComparisonType(rawExpressionMetadata.Type)) {
				return ConvertFilter(rawExpressionMetadata, modelMetadata);
			}

			if (rawExpressionMetadata.Type == ExpressionType.MemberAccess) {
				return ConvertMemberAccessParameter(rawExpressionMetadata, modelMetadata);
			}

			if (rawExpressionMetadata.Type == ExpressionType.Constant) {
				return ConvertConstant(rawExpressionMetadata, modelMetadata);
			}
			throw new NotImplementedException();
		}

		private static ExpressionMetadata ConvertConstant(RawExpressionMetadata rawExpressionMetadata, ExpressionModelMetadata modelMetadata) {
			var response = new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Property,
				Parameter = rawExpressionMetadata.Parameter
			};
			return response;
		}

		private static ExpressionMetadata ConvertMemberAccessParameter(RawExpressionMetadata rawExpressionMetadata, ExpressionModelMetadata modelMetadata) {
			if (rawExpressionMetadata.RawDetailExpressionMetadata != null) {
				return ConvertDetailMemberAccessParameter(rawExpressionMetadata, modelMetadata);
			}

			if (rawExpressionMetadata.DatePart != DatePart.None) {
				return ConvertDatePartMemberAccessParameter(rawExpressionMetadata, modelMetadata);
			}

			return ConvertSimpleMemberAccessParameter(rawExpressionMetadata, modelMetadata);
		}

		private static ExpressionMetadata ConvertDatePartMemberAccessParameter(RawExpressionMetadata rawExpressionMetadata, ExpressionModelMetadata modelMetadata) {
			var response = new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Function,
				Parameter = rawExpressionMetadata.Parameter,
				DatePart = rawExpressionMetadata.DatePart
			};
			return response;
		}

		private static ExpressionMetadata ConvertDetailMemberAccessParameter(RawExpressionMetadata rawExpressionMetadata, ExpressionModelMetadata modelMetadata) {
			var rawDetailExpressionMetadata = rawExpressionMetadata.RawDetailExpressionMetadata;
			var detailChain = ExpressionToMetadataConverter.Convert(
				rawDetailExpressionMetadata.FullExpression, rawDetailExpressionMetadata.DetailProperty);
			return new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Detail,
				Parameter =  rawExpressionMetadata.Parameter,
				MethodName = rawDetailExpressionMetadata.FullExpression.Method.Name,
				DetailChain = detailChain
			};
		}

		private static ExpressionMetadata ConvertSimpleMemberAccessParameter(
			RawExpressionMetadata rawExpressionMetadata, ExpressionModelMetadata modelMetadata) {
			var response = new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Column,
				Parameter = rawExpressionMetadata.Parameter
			};
			return response;
		}

		private static ExpressionMetadata ConvertFilter(RawExpressionMetadata rawExpressionMetadata, ExpressionModelMetadata modelMetadata) {
			if (rawExpressionMetadata.FieldMethod != AvailableFieldMethod.None || rawExpressionMetadata.ColumnMethod != AvailableColumnMethod.None) {
				return ConvertExternalFilter(rawExpressionMetadata, modelMetadata);
			} else {
				return ConvertSimpleFilter(rawExpressionMetadata, modelMetadata);
			}
		}

		private static ExpressionMetadata ConvertExternalFilter(RawExpressionMetadata rawExpressionMetadata, ExpressionModelMetadata modelMetadata) {
			if (rawExpressionMetadata.FieldMethod == AvailableFieldMethod.In) {
				return ConvertInFilter(rawExpressionMetadata, modelMetadata);
			}

			if (rawExpressionMetadata.ColumnMethod == AvailableColumnMethod.Contains ||
				rawExpressionMetadata.ColumnMethod == AvailableColumnMethod.StartWith ||
				rawExpressionMetadata.ColumnMethod == AvailableColumnMethod.EndWith) {
				return ConvertStringPartFilter(rawExpressionMetadata, modelMetadata);
			}

			throw new NotImplementedException();
		}

		private static ExpressionMetadata ConvertStringPartFilter(RawExpressionMetadata rawExpressionMetadata, ExpressionModelMetadata modelMetadata) {
			var comparisonType = ApplyNot(GetComparisonTypeByColumnMethod(rawExpressionMetadata.ColumnMethod), rawExpressionMetadata.IsNot);
			var response = new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Comparison,
				IsNot = rawExpressionMetadata.IsNot,
				ComparisonType = comparisonType,
				LeftExpression = Convert(rawExpressionMetadata.Left, modelMetadata),
				RightExpression = Convert(rawExpressionMetadata.Right, modelMetadata)
			};
			return response;
		}

		private static FilterComparisonType GetComparisonTypeByColumnMethod(AvailableColumnMethod columnMethod) {
			switch (columnMethod) {
				case AvailableColumnMethod.Contains:
					return FilterComparisonType.Contain;
				case AvailableColumnMethod.StartWith:
					return FilterComparisonType.StartWith;
				case AvailableColumnMethod.EndWith:
					return FilterComparisonType.EndWith;
				default:
					throw new ExpressionConvertException();
			}
		}

		private static ExpressionMetadata ConvertInFilter(RawExpressionMetadata rawExpressionMetadata, ExpressionModelMetadata modelMetadata) {
			if (rawExpressionMetadata.Items.IsEmpty()) {
				throw new ExpressionConvertException();
			}
			var comparisonType = GetComparisonType(rawExpressionMetadata);
			var response = new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Comparison,
				ComparisonType = comparisonType,
				LeftExpression = Convert(rawExpressionMetadata.Left, modelMetadata)
			};
			rawExpressionMetadata.Items.ForEach(x=> response.RightExpressions.Add(Convert(x, modelMetadata)));
			return response;
		}

		private static FilterComparisonType GetComparisonType(RawExpressionMetadata rawExpressionMetadata) {
			return GetComparisonType(rawExpressionMetadata.Type, rawExpressionMetadata.IsNot);
		}

		private static FilterComparisonType GetComparisonType(ExpressionType expressionType, bool isNot) {
			var comparisonType = ExpressionConverterUtilities.GetComparisonType(expressionType);
			return ApplyNot(comparisonType, isNot);
		}

		private static FilterComparisonType ApplyNot(FilterComparisonType comparisonType, bool isNot) {
			return isNot
				? ExpressionConverterUtilities.GetNotComparisonType(comparisonType)
				: comparisonType;
		}

		private static ExpressionMetadata ConvertSimpleFilter(RawExpressionMetadata rawExpressionMetadata, ExpressionModelMetadata modelMetadata) {
			if (rawExpressionMetadata.Right.Parameter?.Value == null) {
				return ConvertToNullFilter(rawExpressionMetadata, modelMetadata);
			}
			var comparisonType = GetComparisonType(rawExpressionMetadata);
			var response = new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Comparison,
				ComparisonType = comparisonType,
				LeftExpression = Convert(rawExpressionMetadata.Left, modelMetadata),
				RightExpression = Convert(rawExpressionMetadata.Right, modelMetadata)
			};
			return response;
		}

		private static ExpressionMetadata ConvertToNullFilter(RawExpressionMetadata rawExpressionMetadata, ExpressionModelMetadata modelMetadata) {
			var filterComparisonType = rawExpressionMetadata.Type == ExpressionType.Equal
				? FilterComparisonType.IsNull
				: FilterComparisonType.IsNotNull;
			var comparisonType = ApplyNot(filterComparisonType, rawExpressionMetadata.IsNot);
			var response = new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Comparison,
				ComparisonType = comparisonType,
				LeftExpression = Convert(rawExpressionMetadata.Left, modelMetadata)
			};
			return response;
		}

		private static ExpressionMetadata ConvertFilterGroup(RawExpressionMetadata rawExpressionMetadata, ExpressionModelMetadata modelMetadata) {
			var response = new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Group,
				IsNot = rawExpressionMetadata.IsNot,
				LogicalOperation = ExpressionConverterUtilities.ConvertLogicalOperation(rawExpressionMetadata.Type)
			};
			rawExpressionMetadata.Items.ForEach(x=>response.Items.Add(Convert(x, modelMetadata)));
			return response;
		}
	}
}
