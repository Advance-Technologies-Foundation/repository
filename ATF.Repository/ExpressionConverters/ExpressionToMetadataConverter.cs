using System;
using System.Linq;
using System.Linq.Expressions;

namespace ATF.Repository.ExpressionConverters
{
	internal static class ExpressionToMetadataConverter
	{
		public static ExpressionMetadataChain Convert(Expression expression, Type lastValueType) {
			var chain = ConvertExpressionToChain(expression);
			chain.Items.ForEach(x => {
				x.ExpressionMetadata = CreateExpressionMetadata(x, chain);
			});
			chain.LastValueType = lastValueType;
			return chain;
		}

		private static ExpressionMetadata CreateExpressionMetadata(ExpressionMetadataChainItem expressionMetadataChainItem, ExpressionMetadataChain chain) {
			var expressionModelMetadata = CreateExpressionModelMetadata(expressionMetadataChainItem, chain);
			var response = ExpressionConverter.ConvertModelQueryExpression(expressionMetadataChainItem.Expression, expressionModelMetadata);
			return response;
		}

		private static ExpressionModelMetadata CreateExpressionModelMetadata(ExpressionMetadataChainItem expressionMetadataChainItem, ExpressionMetadataChain chain) {
			var arguments = expressionMetadataChainItem.Expression.Arguments;
			if (arguments.Count < 2 || !(arguments.Skip(1).First() is UnaryExpression unaryExpression)) {
				return new ExpressionModelMetadata() {
					Type = expressionMetadataChainItem.InputDtoType.Type
				};
			}
			if (!(unaryExpression.Operand is LambdaExpression lambdaExpressionOperand)) {
				throw new ArgumentException("Operand is not LambdaExpression");
			}

			var parameter = lambdaExpressionOperand.Parameters.First(x=>x.Type == expressionMetadataChainItem.InputDtoType.Type);

			return new ExpressionModelMetadata() {
				Type = parameter.Type,
				Name = parameter.Name
			};
		}

		private static ExpressionMetadataChain ConvertExpressionToChain(Expression expression, ExpressionMetadataChain chain = null) {
			chain = chain ?? new ExpressionMetadataChain();
			if (!(expression is MethodCallExpression methodCallExpression)) {
				return chain;
			}
			chain.Items.Insert(0, new ExpressionMetadataChainItem(methodCallExpression));
			ConvertExpressionToChain(methodCallExpression.Arguments.First(), chain);
			return chain;
		}
	}
}
