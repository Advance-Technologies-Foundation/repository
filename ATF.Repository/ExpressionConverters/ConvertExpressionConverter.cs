using System;
using System.Linq.Expressions;

namespace ATF.Repository.ExpressionConverters
{
	internal class ConvertExpressionConverter: ExpressionConverter {

		internal override ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			if (TryDynamicInvoke(expression, out var value)) {
				return GetPropertyMetadata(expression, value);
			}
			if (expression is UnaryExpression unaryExpression)
			{
				if (IsColumnPathMember(unaryExpression.Operand, modelMetadata)) {
					return GetColumnPathMetadata(unaryExpression.Operand, modelMetadata);
				}
			}
			throw new NotImplementedException();
		}
	}
}
