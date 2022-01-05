namespace ATF.Repository.ExpressionConverters
{
	using System.Linq.Expressions;

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
