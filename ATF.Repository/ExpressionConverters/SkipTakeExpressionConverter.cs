using System;
using System.Linq;
using System.Linq.Expressions;
using ATF.Repository.Exceptions;

namespace ATF.Repository.ExpressionConverters
{
	internal class SkipTakeExpressionConverter: ExpressionConverter {

		internal override ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			if (!(expression is MethodCallExpression methodCallExpression) || methodCallExpression.Arguments.Count < 2) {
				throw new ExpressionConvertException();
			}

			var body = methodCallExpression.Arguments.Skip(1).First();
			if (TryDynamicInvoke(body, out var value)) {
				return GetPropertyMetadata(body, value);
			}
			throw new NotSupportedException();
		}
	}
}
