namespace ATF.Repository.ExpressionConverters
{
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using Terrasoft.Core.Entities;

	internal class LambdaConverter : ExpressionConverter
	{
		private readonly LambdaExpression _node;
		public LambdaConverter(LambdaExpression node) {
			_node = node;
		}

		internal override ExpressionMetadata ConvertNode() {
			if (_node.Body.NodeType == ExpressionType.MemberAccess && _node.Body.Type == typeof(bool)) {
				return GenerateUnaryBooleanComparisonFilterMetadata((MemberExpression)_node.Body);
			}

			if (_node.Body.NodeType == ExpressionType.Not && IsUnaryNotMemberExpression(_node.Body)) {
				var unaryExpression = (UnaryExpression) _node.Body;
				if (unaryExpression.Operand is MemberExpression memberExpression) {
					var memberExpressionMetaData = GenerateUnaryBooleanComparisonFilterMetadata(memberExpression);
					return NotExpressionConverter.ApplyNot(memberExpressionMetaData);
				}

				var operandExpressionMetadata = ConvertNode(unaryExpression.Operand, modelMetadata);
				return NotExpressionConverter.ApplyNot(operandExpressionMetadata);
			}
			return ConvertNode(_node.Body, modelMetadata);
		}

		private bool IsUnaryNotMemberExpression(Expression nodeBody) {
			return nodeBody is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Not;
		}

		private ExpressionMetadata GenerateUnaryBooleanComparisonFilterMetadata(MemberExpression member) {
			var left = ConvertNode(member, modelMetadata);
			var right = GetPropertyMetadata(member, true);
			return new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Comparison,
				LeftExpression = left,
				RightExpressions = new List<ExpressionMetadata>() { right },
				ComparisonType = FilterComparisonType.Equal
			};
		}
	}
}
