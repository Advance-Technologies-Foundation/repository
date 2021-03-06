﻿using System;
using System.Linq.Expressions;

namespace ATF.Repository.ExpressionConverters
{
	internal class NotExpressionConverter: ExpressionConverter {

		internal override ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			if (TryDynamicInvoke(expression, out var value)) {
				return GetPropertyMetadata(expression, value);
			}
			if (expression is UnaryExpression unaryExpression) {
				var response = ConvertModelQueryExpression(unaryExpression.Operand, modelMetadata);
				response.IsNot = true;
				return response;
			}
			throw new NotImplementedException();
		}
	}
}
