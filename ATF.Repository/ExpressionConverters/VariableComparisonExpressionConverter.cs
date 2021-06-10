using System.Linq.Expressions;
using ATF.Repository.Exceptions;
using Terrasoft.Common;

namespace ATF.Repository.ExpressionConverters
{
	internal class VariableComparisonExpressionConverter: ComparisonExpressionConverter {
		internal override ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			if (!(expression is MethodCallExpression methodCallExpression)) {
				throw new ExpressionConvertException();
			}

			return methodCallExpression.Arguments.Count > 1
				? base.Convert(expression, modelMetadata)
				: GenerateGroup(LogicalOperationStrict.And);
		}
	}
}
