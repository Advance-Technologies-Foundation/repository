using System;
using System.Linq;
using System.Linq.Expressions;
using ATF.Repository.ExpressionConverters;
using ATF.Repository.Queryables;
using Terrasoft.Common;
using Terrasoft.Nui.ServiceModel.DataContract;

namespace ATF.Repository.ExpressionAppliers
{
	internal class OrderMethodApplier : ExpressionApplier
	{
		internal override bool Apply(ExpressionChainItem expressionChainItem, ModelQueryBuildConfig config) {
			var converter = new InitialCallExpressionConverter(expressionChainItem.Expression);
			var filterMetadata = converter.ConvertNode();
			var orderedColumn = GetOrderedColumn(filterMetadata, config);
			ApplyOrderDirectionAndOrderPosition(orderedColumn, expressionChainItem.Expression, config);
			return true;
		}

		private void ApplyOrderDirectionAndOrderPosition(SelectQueryColumn orderedColumn,
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

		private SelectQueryColumn GetOrderedColumn(ExpressionMetadata filterMetadata, ModelQueryBuildConfig config) {
			var columnFilterMetadata = filterMetadata.Items.FirstOrDefault();
			if (columnFilterMetadata == null || columnFilterMetadata.NodeType != ExpressionMetadataNodeType.Column) {
				throw new NotSupportedException();
			}

			return GetOrAddColumn(config, columnFilterMetadata.Parameter.ColumnPath);
		}
	}
}
