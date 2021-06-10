using ATF.Repository.ExpressionConverters;
using ATF.Repository.Queryables;

namespace ATF.Repository.ExpressionAppliers
{
	internal class FirstMethodApplier: WhereMethodApplier
	{
		internal override bool Apply(ExpressionMetadataChainItem expressionMetadataChainItem, ModelQueryBuildConfig config) {
			if (expressionMetadataChainItem.Expression.Arguments.Count <= 1 || !base.Apply(expressionMetadataChainItem, config)) {
				return false;
			}
			config.SelectQuery.RowCount = 1;
			return true;
		}
	}
}
