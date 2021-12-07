using System.Linq;
using System.Linq.Expressions;
using ATF.Repository.Exceptions;
using ATF.Repository.ExpressionConverters;

namespace ATF.Repository.ExpressionConverters
{
	internal class VariableExpressionConverter
	{
		internal static ExpressionMetadata Convert(MethodCallExpression methodCallExpression, ExpressionModelMetadata modelMetadata) {
			if (methodCallExpression.Arguments.Count > 1) {
				return WhereExpressionConverter.Convert(methodCallExpression, modelMetadata);
			}
			return new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Group
			};
		}
	}
}
