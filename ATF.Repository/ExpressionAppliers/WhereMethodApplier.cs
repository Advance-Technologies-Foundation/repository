using System;
using ATF.Repository.ExpressionConverters;
using ATF.Repository.Queryables;

namespace ATF.Repository.ExpressionAppliers
{
	internal class WhereMethodApplier : ExpressionApplier
	{
		internal override bool Apply(ExpressionMetadataChainItem expression, ModelQueryBuildConfig config) {
			var filter = ModelQueryFilterBuilder.GenerateFilter(expression.ExpressionMetadata);

			config.SelectQuery.Filters.Items.Add(Guid.NewGuid().ToString(), filter);
			return true;
		}
	}
}
