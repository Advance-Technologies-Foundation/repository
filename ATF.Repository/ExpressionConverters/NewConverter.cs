namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Linq.Expressions;

	internal class NewConverter : ExpressionConverter
	{
		private readonly Expression _node;
		public NewConverter(Expression node) {
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
