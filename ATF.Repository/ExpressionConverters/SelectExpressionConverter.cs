namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Linq.Expressions;

	internal static class SelectExpressionConverter
	{
		internal static ExpressionMetadata Convert(MethodCallExpression expression, ExpressionModelMetadata modelMetadata) {
			if (ExpressionConverterUtilities.TryDynamicInvoke(expression, out var value)) {
				return ExpressionConverterUtilities.GetPropertyMetadata(expression, value);
			}

			throw new NotImplementedException();
		}
	}
}
