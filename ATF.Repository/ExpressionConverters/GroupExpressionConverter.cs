namespace ATF.Repository.ExpressionConverters
{
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using Terrasoft.Common;

	internal class GroupExpressionConverter : ExpressionConverter
	{
		private readonly BinaryExpression _node;
		public GroupExpressionConverter(BinaryExpression node) {
			_node = node;
		}

		internal override ExpressionMetadata ConvertNode() {
			var left = ConvertNode(_node.Left, modelMetadata);
			var right = ConvertNode(_node.Right, modelMetadata);
			var metaData = new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Group,
				LogicalOperation = GetLogicalOperation(),
				Items = new List<ExpressionMetadata>()
			};
			if (left != null) {
				metaData.Items.Add(left);
			}
			if (right != null) {
				metaData.Items.Add(right);
			}
			return metaData;
		}

		private LogicalOperationStrict GetLogicalOperation() {
			return _node.NodeType == ExpressionType.Add || _node.NodeType == ExpressionType.AndAlso
				? LogicalOperationStrict.And
				: LogicalOperationStrict.Or;
		}
	}
}
