using System;
using System.Linq.Expressions;
using ATF.Repository.ExpressionConverters;

namespace ATF.Repository.ExpressionConverters
{
	internal static class RawExpressionMetadataNormalizer
	{
		internal static RawExpressionMetadata Normalize(RawExpressionMetadata rawExpressionMetadata) {
			if (ExpressionConverterUtilities.IsLogicalOperationExpressionType(rawExpressionMetadata.Type)) {
				return NormalizeAsFilterGroup(rawExpressionMetadata);
			}

			if (ExpressionConverterUtilities.IsComparisonType(rawExpressionMetadata.Type)) {
				return NormalizeAsCondition(rawExpressionMetadata);
			}

			if (rawExpressionMetadata.Type == ExpressionType.MemberAccess) {
				return NormalizeAsFilterItem(rawExpressionMetadata);
			}

			throw new NotImplementedException();
		}

		private static RawExpressionMetadata NormalizeAsCondition(RawExpressionMetadata rawExpressionMetadata) {
			var correctDataValueType = rawExpressionMetadata.Left.Parameter.Type;
			if (rawExpressionMetadata.Right != null &&
				rawExpressionMetadata.Right.Parameter.Type != correctDataValueType) {
				rawExpressionMetadata.Right.Parameter.Type = correctDataValueType;
			}
			rawExpressionMetadata.Items.ForEach(x=>x.Parameter.Type = correctDataValueType);
			return rawExpressionMetadata;
		}

		private static RawExpressionMetadata NormalizeAsFilterGroup(RawExpressionMetadata rawExpressionMetadata) {
			rawExpressionMetadata.Items.Add(NormalizeAsFilterItem(rawExpressionMetadata.Left));
			rawExpressionMetadata.Items.Add(NormalizeAsFilterItem(rawExpressionMetadata.Right));
			rawExpressionMetadata.Left = null;
			rawExpressionMetadata.Right = null;
			return rawExpressionMetadata;
		}

		private static RawExpressionMetadata NormalizeAsFilterItem(RawExpressionMetadata rawExpressionMetadata) {
			if (rawExpressionMetadata.Type == ExpressionType.MemberAccess) {
				return Normalize(ConvertMemberAccessToFilterItem(rawExpressionMetadata));
			}

			if (ExpressionConverterUtilities.IsComparisonType(rawExpressionMetadata.Type)) {
				return Normalize(rawExpressionMetadata);
			}

			if (ExpressionConverterUtilities.IsLogicalOperationExpressionType(rawExpressionMetadata.Type)) {
				return Normalize(rawExpressionMetadata);
			}

			throw new NotImplementedException();
		}

		private static RawExpressionMetadata ConvertMemberAccessToFilterItem(RawExpressionMetadata memberRawExpressionMetadata) {
			if (memberRawExpressionMetadata.Parameter.Type == typeof(bool)) {
				return ConvertBooleanMemberAccessToFilterItem(memberRawExpressionMetadata);
			}

			throw new NotSupportedException();
		}

		private static RawExpressionMetadata ConvertBooleanMemberAccessToFilterItem(RawExpressionMetadata memberRawExpressionMetadata) {
			return new RawExpressionMetadata() {
				Left = memberRawExpressionMetadata,
				IsNot = memberRawExpressionMetadata.IsNot,
				Type = ExpressionType.Equal,
				Right = new RawExpressionMetadata() {
					Type = ExpressionType.Constant,
					Parameter = new ExpressionMetadataParameter() {Type = typeof(bool), Value = true}
				}
			};
		}
	}
}
