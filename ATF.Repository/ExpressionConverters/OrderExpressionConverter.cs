using System;
using System.Linq;
using System.Linq.Expressions;
using ATF.Repository.Exceptions;
using ATF.Repository.ExpressionConverters;

namespace ATF.Repository.ExpressionConverters
{
	internal static class OrderExpressionConverter
	{
		internal static ExpressionMetadata Convert(MethodCallExpression expression, ExpressionModelMetadata modelMetadata) {
			var body = ExpressionConverterUtilities.GetSecondArgumentExpression(expression);
			if (!ExpressionConverterUtilities.TryGetColumnMemberPath(body, modelMetadata, out var path)) {
				throw new NotSupportedException();
			}
			var response = ExpressionConverterUtilities.GetColumnMetadata(body.Type, path);
			response.MethodName = expression.Method.Name;
			return response;
		}
	}
}
