using System.Linq.Expressions;
using ATF.Repository.Exceptions;

namespace ATF.Repository.ExpressionConverters
{
	internal class MemberAccessExpressionConverter: ExpressionConverter {

		internal override ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			if (!(expression is MemberExpression memberExpression)) {
				throw new ExpressionConvertException();
			}

			if (IsColumnPathMember(memberExpression, modelMetadata)) {
				return GetColumnPathMetadata(memberExpression, modelMetadata);
			}

			if (TryDynamicInvoke(memberExpression, out var value)) {
				return GetPropertyMetadata(memberExpression, value);
			}

			return null;
		}
	}
}
