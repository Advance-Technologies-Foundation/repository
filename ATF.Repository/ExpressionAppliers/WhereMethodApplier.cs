using System;
using System.Linq;
using System.Linq.Expressions;
using ATF.Repository.ExpressionConverters;
using ATF.Repository.Queryables;

namespace ATF.Repository.ExpressionAppliers
{
	internal class WhereMethodApplier : ExpressionApplier
	{
		internal override bool Apply(ExpressionChainItem expressionChainItem, ModelQueryBuildConfig config) {
			var converter = new InitialCallExpressionConverter(expressionChainItem.Expression);
			var filterMetadata = converter.ConvertNode();
			var necessaryFilterMetadata = SkipUnnecessaryItems(filterMetadata);
			var filter = ModelQueryFilterBuilder.GenerateFilter(necessaryFilterMetadata);
			config.SelectQuery.Filters.Items.Add(Guid.NewGuid().ToString(), filter);
			return true;
		}

		private static ExpressionMetadata SkipUnnecessaryItems(ExpressionMetadata expressionMetadata) {
			return expressionMetadata.NodeType == ExpressionMetadataNodeType.Group &&
			       expressionMetadata.Items != null && expressionMetadata.Items.Count == 1 &&
			       expressionMetadata.Items.First().NodeType == ExpressionMetadataNodeType.Group
				? SkipUnnecessaryItems(expressionMetadata.Items.First())
				: expressionMetadata;
		}
	}
}
