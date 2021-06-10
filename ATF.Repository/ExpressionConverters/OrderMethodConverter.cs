using System;
using System.Linq.Expressions;

namespace ATF.Repository.ExpressionConverters
{
	internal class OrderMethodConverter: ExpressionConverter {
		internal override ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			var body = GetExpressionBody(expression);
			if (IsColumnPathMember(body, modelMetadata)) {
				return GetColumnPathMetadata(body, modelMetadata);
			}
			throw new NotSupportedException();
		}
	}
}
