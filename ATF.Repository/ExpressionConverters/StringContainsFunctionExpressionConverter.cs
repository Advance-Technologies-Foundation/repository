namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Linq;
	using System.Linq.Expressions;
	using Terrasoft.Core.Entities;

	internal class StringContainsFunctionExpressionConverter : FunctionExpressionConverter
	{
		public StringContainsFunctionExpressionConverter(MethodCallExpression expression) : base(expression) {
		}
		internal override ExpressionMetadata ConvertNode() {
			if (IsColumnPathMember(Node.Object)) {
				return ComparisonConverter.GenerateComparisonExpressionMetadata(Node.Object,
					FilterComparisonType.Contain, Node.Arguments.FirstOrDefault(), modelMetadata);
			}
			throw new NotImplementedException();
		}

	}
}
