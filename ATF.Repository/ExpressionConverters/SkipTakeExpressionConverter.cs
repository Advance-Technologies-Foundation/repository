namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Linq;
	using System.Linq.Expressions;
	using ATF.Repository.Exceptions;

	internal class SkipTakeExpressionConverter: BaseExpressionConverter
	{
		internal static ExpressionMetadata Convert(MethodCallExpression methodCallExpression, ExpressionModelMetadata modelMetadata) {
			if (methodCallExpression.Arguments.Count < 2) {
				throw new ExpressionConvertException();
			}

			var body = methodCallExpression.Arguments.Skip(1).First();
			if (!ExpressionConverterUtilities.TryDynamicInvoke(body, out var value)) {
				throw new NotSupportedException();
			}

			var expressionMetadata = ExpressionConverterUtilities.GetPropertyMetadata(body, value);
			expressionMetadata.MethodName = methodCallExpression.Method.Name;
			return expressionMetadata;
		}
	}
}
