using System;
using System.Linq.Expressions;
using ATF.Repository.Exceptions;

namespace ATF.Repository.ExpressionConverters
{
	internal class LambdaExpressionConverter: ExpressionConverter {

		internal override ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			if (!(expression is LambdaExpression lambdaExpression)) {
				throw new ExpressionConvertException();
			}

			throw new NotImplementedException();
		}
	}
}
