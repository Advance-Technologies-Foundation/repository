using System;
using System.Linq;
using System.Linq.Expressions;
using ATF.Repository.ExpressionConverters;
using ATF.Repository.Queryables;

namespace ATF.Repository.ExpressionAppliers
{
	internal class TakeMethodApplier : ExpressionApplier
	{
		internal override bool Apply(ExpressionMetadataChainItem expressionMetadataChainItem, ModelQueryBuildConfig config) {
			if (expressionMetadataChainItem.ExpressionMetadata.NodeType != ExpressionMetadataNodeType.Property)
				return false;
			var rowCount = (int)(expressionMetadataChainItem?.ExpressionMetadata?.Parameter?.Value ?? 0);
			config.SelectQuery.RowCount = Math.Min(ModelQueryBuildConfig.MaxRowsCount, rowCount);
			return true;

		}

	}
}
