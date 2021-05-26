namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;

	internal class MemberAccessConverter : ExpressionConverter
	{
		private readonly MemberExpression _node;
		public MemberAccessConverter(MemberExpression node) {
			_node = node;
		}

		internal override ExpressionMetadata ConvertNode() {
			if (IsColumnPathMember(_node)) {
				return GetColumnPathMetadata(_node);
			}
			if (TryDynamicInvoke(_node, out var value)) {
				return GetPropertyMetadata(_node, value);
			}

			return null;
		}



	}
}
