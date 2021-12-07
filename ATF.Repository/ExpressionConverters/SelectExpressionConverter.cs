using System;
using System.Linq.Expressions;
using ATF.Repository.ExpressionConverters;

namespace ATF.Repository.ExpressionConverters
{
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
