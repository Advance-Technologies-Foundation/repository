namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Linq.Expressions;

	internal class InitialCallExpressionConverter : ExpressionConverter
	{
		private readonly MethodCallExpression _node;

		internal InitialCallExpressionConverter(MethodCallExpression node) {
			_node = node;
		}
		internal override ExpressionMetadata ConvertNode() {
			var metadata = new ExpressionMetadata {
				NodeType = ExpressionMetadataNodeType.Group,
				ModelMetadata = ParseModelMetadata(_node.Arguments)
			};
			foreach (var arg in _node.Arguments.Skip(1)) {
				var itemMetadata = ConvertNode(arg, metadata.ModelMetadata);
				if (itemMetadata != null) {
					metadata.Items.Add(itemMetadata);
				}
			}
			return metadata;
		}

		private ExpressionModelMetadata ParseModelMetadata(ReadOnlyCollection<Expression> nodeArguments) {
			var first = nodeArguments.First();
			var propertyType = first.Type.GenericTypeArguments.First();
			var second = nodeArguments.Skip(1).First();
			if (!(second is UnaryExpression unaryExpression)) {
				throw new ArgumentException("Argument is not UnaryExpression type", nameof(second));
			}
			if (!(unaryExpression.Operand is LambdaExpression lambdaExpressionOperand)) {
				throw new ArgumentException("Operand is not LambdaExpression", nameof(second));
			}

			var parameter = lambdaExpressionOperand.Parameters.First(x=>x.Type == propertyType);

			return new ExpressionModelMetadata() {
				Type = parameter.Type,
				Name = parameter.Name
			};
		}

	}
}
