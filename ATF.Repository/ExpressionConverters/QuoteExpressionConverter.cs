namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Linq.Expressions;

	internal class QuoteExpressionConverter : ExpressionConverter
	{
		private readonly Expression _node;
		public QuoteExpressionConverter(Expression node) {
			_node = node;
		}

		internal override ExpressionMetadata ConvertNode() {
			if (_node is UnaryExpression unaryExpression) {
				return ConvertQuoteUnaryExpression(unaryExpression);
			}
			throw new NotImplementedException();
		}

		private ExpressionMetadata ConvertQuoteUnaryExpression(UnaryExpression unaryExpression) {
			return ConvertNode(unaryExpression.Operand, modelMetadata);
		}
	}
}
