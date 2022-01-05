namespace ATF.Repository.ExpressionAppliers
{
	using System;
	using ATF.Repository.ExpressionConverters;
	using ATF.Repository.Queryables;

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
