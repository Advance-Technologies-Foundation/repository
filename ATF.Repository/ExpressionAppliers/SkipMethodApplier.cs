namespace ATF.Repository.ExpressionAppliers
{
	using System;
	using ATF.Repository.ExpressionConverters;
	using ATF.Repository.Queryables;

	internal class SkipMethodApplier : ExpressionApplier
	{
		internal override bool Apply(ExpressionMetadataChainItem expressionMetadataChainItem, ModelQueryBuildConfig config) {
			if (expressionMetadataChainItem.ExpressionMetadata.NodeType != ExpressionMetadataNodeType.Property)
				return false;
			var rowsOffset = (int)(expressionMetadataChainItem?.ExpressionMetadata?.Parameter?.Value ?? 0);
			config.SelectQuery.RowsOffset = Math.Max(0, rowsOffset);
			return true;
		}
	}
}
