using System;
using System.Linq.Expressions;

namespace ATF.Repository.ExpressionConverters
{
	internal class ConstantExpressionConverter: ExpressionConverter {

		internal override ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			if (TryDynamicInvoke(expression, out var value)) {
				return GetPropertyMetadata(expression, value);
			}
			throw new NotSupportedException();
		}
	}
}
