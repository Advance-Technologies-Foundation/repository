namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Linq.Expressions;

	internal class ConstantConverter : ExpressionConverter
	{
		private readonly ConstantExpression _node;
		public ConstantConverter(ConstantExpression node) {
			_node = node;
		}

		internal override ExpressionMetadata ConvertNode() {
			if (TryDynamicInvoke(_node, out var value)) {
				return GetPropertyMetadata(_node, value);
			}
			throw new NotImplementedException();
		}
	}
}
