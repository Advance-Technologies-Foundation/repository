using System.Linq.Expressions;
using ATF.Repository.Queryables;

namespace ATF.Repository.ExpressionAppliers
{
	internal class FirstMethodApplier: WhereMethodApplier
	{
		internal override bool Apply(ExpressionChainItem expressionChainItem, ModelQueryBuildConfig config) {

			if (expressionChainItem.Expression.Arguments.Count <= 1 || !base.Apply(expressionChainItem, config)) {
				return false;
			}
			config.SelectQuery.RowCount = 1;
			return true;
		}
	}
}
