namespace ATF.Repository.ExpressionAppliers
{
	using System.Linq;
	using System.Linq.Expressions;
	using ATF.Repository.ExpressionConverters;
	using ATF.Repository.Queryables;
	using ATF.Repository.Replicas;
	using Terrasoft.Common;

	internal class OrderMethodApplier : ExpressionApplier
	{
		internal override bool Apply(ExpressionMetadataChainItem expressionMetadataChainItem, ModelQueryBuildConfig config) {
			if (expressionMetadataChainItem.ExpressionMetadata.NodeType != ExpressionMetadataNodeType.Column)
				return false;

			var orderedColumn = GetOrAddColumn(config, expressionMetadataChainItem.ExpressionMetadata.Parameter.ColumnPath);
			ApplyOrderDirectionAndOrderPosition((SelectQueryColumnReplica)orderedColumn, expressionMetadataChainItem.Expression, config);
			return true;
		}

		private void ApplyOrderDirectionAndOrderPosition(SelectQueryColumnReplica orderedColumn,
			MethodCallExpression expression, ModelQueryBuildConfig config) {
			var position = GetOrderedColumnsCount(config) + 1;
			var direction = GetOrderDirection(expression);
			orderedColumn.OrderPosition = position;
			orderedColumn.OrderDirection = direction;
		}

		private OrderDirection GetOrderDirection(MethodCallExpression expression) {
			return expression.Method.Name.EndsWith("Descending")
				? OrderDirection.Descending
				: OrderDirection.Ascending;
		}

		private int GetOrderedColumnsCount(ModelQueryBuildConfig config) {
			return config.SelectQuery.Columns.Items.Count(pair => pair.Value.OrderPosition > -1);
		}
	}
}
