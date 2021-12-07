using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace ATF.Repository.ExpressionConverters
{
	internal static class ExpressionToMetadataConverter
	{
		internal static ExpressionMetadataChain Convert(Expression expression, Type lastValueType) {
			var chain = ConvertExpressionToChain(expression);
			chain.LastValueType = lastValueType;
			Convert(chain);
			chain.ValidateChainItems(AvailableChainMethods.MainMethods);
			return chain;
		}

		internal static ExpressionMetadataChain Convert(Expression expression, MemberExpression collectionRootExpression) {
			var chain = ConvertExpressionToChain(expression);
			chain.LastValueType = GetTypeFromCollectionRootExpression(collectionRootExpression);
			Convert(chain);
			chain.ValidateChainItems(AvailableChainMethods.DetailMethods);
			return chain;
		}

		private static Type GetTypeFromCollectionRootExpression(MemberExpression chainToExpression) {
			return chainToExpression.Type.GenericTypeArguments?.FirstOrDefault();
		}

		private static void Convert(ExpressionMetadataChain chain) {
			chain.Items.ForEach(x => {
				x.ExpressionMetadata = CreateExpressionMetadata(x, chain);
			});
		}

		private static ExpressionMetadata CreateExpressionMetadata(ExpressionMetadataChainItem expressionMetadataChainItem, ExpressionMetadataChain chain) {
			var expressionModelMetadata = CreateExpressionModelMetadata(expressionMetadataChainItem, chain);
			var response = ExpressionConverter.ConvertMetadataChainItemExpressionMetadata(expressionMetadataChainItem,
				expressionModelMetadata);
			return response;
		}

		private static ExpressionModelMetadata CreateExpressionModelMetadata(ExpressionMetadataChainItem expressionMetadataChainItem, ExpressionMetadataChain chain) {
			ReadOnlyCollection<ParameterExpression> parameters = null;
			var arguments = expressionMetadataChainItem.Expression.Arguments;

			if (arguments.Count < 2 || (parameters = GetModelMetadataParameterExpression(arguments.Skip(1).First())) == null) {
				return new ExpressionModelMetadata() {
					Type = expressionMetadataChainItem.InputDtoType.Type
				};
			}

			var parameter = parameters.First(x=>x.Type == expressionMetadataChainItem.InputDtoType.Type);
			return new ExpressionModelMetadata() {
				Type = parameter.Type,
				Name = parameter.Name
			};
		}

		private static ReadOnlyCollection<ParameterExpression> GetModelMetadataParameterExpression(Expression expression) {
			if (expression is UnaryExpression unaryExpression && unaryExpression.Operand is LambdaExpression innerLambdaExpression) {
				return innerLambdaExpression.Parameters;
			}

			if (expression is LambdaExpression lambdaExpression) {
				return lambdaExpression.Parameters;
			}

			return null;
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
