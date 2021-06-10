using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ATF.Repository.Exceptions;
using Terrasoft.Core.Entities;

namespace ATF.Repository.ExpressionConverters
{
	internal class FieldContainsPropertyMethodConverter: ExpressionConverter {
		internal override ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			if (!(expression is MethodCallExpression methodCallExpression) || methodCallExpression.Arguments.Count < 1) {
				throw new ExpressionConvertException();
			}

			var left = ComparisonExpressionConverter.GenerateFilterElementExpression(
				methodCallExpression.Arguments.First(), modelMetadata);
			var valueType = left.Parameter.Type;
			var valueList = GetRightValuesList(methodCallExpression.Object, GetType(), valueType);
			var rightExpressions = valueList.Select(value => GetPropertyMetadata(valueType, value)).ToArray();
			return ComparisonExpressionConverter.GenerateComparisonExpressionMetadata(left, FilterComparisonType.Equal, rightExpressions);
		}

		private static List<object> GetRightValuesList(Expression collectionExpression, Type converterType, Type valueType) {
			if (!TryDynamicInvoke(collectionExpression, out var collection)) {
				return null;
			}
			var method = RepositoryReflectionUtilities.GetGenericMethod(converterType, "ConvertCollectionToObjectList",
				valueType);
			var list = (List<object>) method.Invoke(null, new object[] { collection });
			return list;
		}

		private static List<object> ConvertCollectionToObjectList<T>(IList<T> original) {
			return original.Select(x => (object) x).ToList();
		}
	}
}
