namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Linq.Expressions;

	public static class SimpleAggregationExpressionConverter
	{
		internal static ExpressionMetadata Convert(MethodCallExpression expression, ExpressionModelMetadata modelMetadata) {
			var body = ExpressionConverterUtilities.GetSecondArgumentExpression(expression);
			if (!ExpressionConverterUtilities.TryGetColumnMemberPath(body, modelMetadata, out var path)) {
				throw new NotSupportedException();
			}
			var response = ExpressionConverterUtilities.GetColumnMetadata(body.Type, path);
			response.MethodName = expression.Method.Name;
			return response;
		}
	}
}
