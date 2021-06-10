using System.Linq;
using System.Linq.Expressions;
using ATF.Repository.Queryables;

namespace ATF.Repository.ExpressionConverters
{
	internal class ExpressionMetadataChainItem
	{
		internal ExpressionMetadataChainItem(MethodCallExpression methodCallExpression) {
			Expression = methodCallExpression;
			InputDtoType = new ExpressionChainDtoType(methodCallExpression.Arguments.First().Type);
			OutputDtoType = new ExpressionChainDtoType(methodCallExpression.Type);
		}
		public MethodCallExpression Expression { get; }

		public ExpressionMetadata ExpressionMetadata { get; set; }
		public ExpressionChainDtoType InputDtoType { get; }
		public ExpressionChainDtoType OutputDtoType { get; }

		public bool IsAppliedToQuery { get; set; }
	}
}
