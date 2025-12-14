namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Linq.Expressions;

	internal static class ExpressionConverter
	{
		internal static ExpressionMetadata ConvertMetadataChainItemExpressionMetadata(ExpressionMetadataChainItem chainItem, ExpressionModelMetadata modelMetadata) {

			if (chainItem.Expression is MethodCallExpression methodCallExpression) {
				return ConvertMethodCallExpressionToExpressionMetadata(methodCallExpression, modelMetadata);
			}
			throw new NotImplementedException();

		}

		private static ExpressionMetadata ConvertMethodCallExpressionToExpressionMetadata(MethodCallExpression methodCallExpression, ExpressionModelMetadata modelMetadata) {
			var methodName = methodCallExpression.Method.Name;
			if (methodName == ConvertableExpressionMethod.Skip || methodName == ConvertableExpressionMethod.Take) {
				return SkipTakeExpressionConverter.Convert(methodCallExpression, modelMetadata);
			}

			if (methodName == ConvertableExpressionMethod.Where) {
				return WhereExpressionConverter.Convert(methodCallExpression, modelMetadata);
			}

			if (methodName == ConvertableExpressionMethod.OrderBy ||
				methodName == ConvertableExpressionMethod.OrderByDescending ||
				methodName == ConvertableExpressionMethod.ThenBy ||
				methodName == ConvertableExpressionMethod.ThenByDescending) {
				return OrderExpressionConverter.Convert(methodCallExpression, modelMetadata);
			}
			if (methodName == ConvertableExpressionMethod.First ||
				methodName == ConvertableExpressionMethod.FirstOrDefault ||
				methodName == ConvertableExpressionMethod.Any ||
				methodName == ConvertableExpressionMethod.Count) {
				return VariableExpressionConverter.Convert(methodCallExpression, modelMetadata);
			}

			if (methodName == ConvertableExpressionMethod.Min ||
				methodName == ConvertableExpressionMethod.Max ||
				methodName == ConvertableExpressionMethod.Sum ||
				methodName == ConvertableExpressionMethod.Average) {
				return SimpleAggregationExpressionConverter.Convert(methodCallExpression, modelMetadata);
			}

			if (methodName == ConvertableExpressionMethod.Select) {
				return SelectExpressionConverter.Convert(methodCallExpression, modelMetadata);
			}

			if (methodName == ConvertableExpressionMethod.GroupBy) {
				return GroupByExpressionConverter.Convert(methodCallExpression, modelMetadata);
			}

			throw new NotImplementedException();
		}
	}
}
