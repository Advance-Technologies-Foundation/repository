namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Linq.Expressions;

	internal class DataTypeConvertConverter : ExpressionConverter
	{
		private readonly UnaryExpression _node;
		public DataTypeConvertConverter(UnaryExpression node) {
			_node = node;
		}

		internal override ExpressionMetadata ConvertNode() {
			if (TryDynamicInvoke(_node, out var value)) {
				return GetPropertyMetadata(_node, value);
			}

			if (IsColumnPathMember(_node.Operand)) {
				return GetColumnPathMetadata(_node.Operand);
			}
			throw new NotImplementedException();
		}
	}
}
