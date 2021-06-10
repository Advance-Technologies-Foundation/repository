using System;
using System.Linq;
using System.Linq.Expressions;
using ATF.Repository.Exceptions;
using Terrasoft.Core.Entities;

namespace ATF.Repository.ExpressionConverters
{
	internal abstract class PropertyMethodConverter: ExpressionConverter {
		internal static ExpressionMetadata ConvertPropertyMethod(Expression expression, ExpressionModelMetadata modelMetadata, FilterComparisonType filterComparisonType) {
			if (!(expression is MethodCallExpression methodCallExpression)) {
				throw new ExpressionConvertException();
			}
			if (IsColumnPathMember(methodCallExpression.Object, modelMetadata)) {
				return ComparisonExpressionConverter.GenerateComparisonExpressionMetadata(methodCallExpression.Object,
					filterComparisonType, modelMetadata, methodCallExpression.Arguments.FirstOrDefault());
			}
			throw new NotSupportedException();
		}
	}

	internal class StringStartWithPropertyMethodConverter: PropertyMethodConverter {
		internal override ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			return ConvertPropertyMethod(expression, modelMetadata, FilterComparisonType.StartWith);
		}
	}

	internal class StringEndWithPropertyMethodConverter: PropertyMethodConverter {
		internal override ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			return ConvertPropertyMethod(expression, modelMetadata, FilterComparisonType.EndWith);
		}
	}

	internal class StringContainsPropertyMethodConverter: PropertyMethodConverter {
		internal override ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			return ConvertPropertyMethod(expression, modelMetadata, FilterComparisonType.Contain);
		}
	}
}
