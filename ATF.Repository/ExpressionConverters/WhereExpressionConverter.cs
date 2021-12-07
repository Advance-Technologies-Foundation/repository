using System;
using System.Linq;
using System.Linq.Expressions;
using ATF.Repository.Exceptions;
using ATF.Repository.ExpressionConverters;

namespace ATF.Repository.ExpressionConverters
{
	public class WhereExpressionConverter
	{
		internal static ExpressionMetadata Convert(MethodCallExpression methodCallExpression, ExpressionModelMetadata modelMetadata) {
			if (methodCallExpression.Arguments.Count < 2) {
				throw new ExpressionConvertException();
			}

			var body = methodCallExpression.Arguments.Skip(1).First();
			var expressionMetadata = FilterConverter.Convert(body, modelMetadata);
			expressionMetadata.MethodName = methodCallExpression.Method.Name;
			return expressionMetadata;
		}
	}
}
