namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using Terrasoft.Core.Entities;

	internal class NotExpressionConverter : ExpressionConverter
	{
		private Expression _node;
		private static Dictionary<FilterComparisonType, FilterComparisonType> NotType = new Dictionary<FilterComparisonType, FilterComparisonType>() {
			{FilterComparisonType.Equal, FilterComparisonType.NotEqual},
			{FilterComparisonType.NotEqual, FilterComparisonType.Equal},
			{FilterComparisonType.Greater, FilterComparisonType.LessOrEqual},
			{FilterComparisonType.GreaterOrEqual, FilterComparisonType.Less},
			{FilterComparisonType.Less, FilterComparisonType.GreaterOrEqual},
			{FilterComparisonType.LessOrEqual, FilterComparisonType.Greater},
			{FilterComparisonType.StartWith, FilterComparisonType.NotStartWith},
			{FilterComparisonType.EndWith, FilterComparisonType.NotEndWith},
			{FilterComparisonType.Contain, FilterComparisonType.NotContain},
		};
		public NotExpressionConverter(Expression node) {
			_node = node;
		}

		internal override ExpressionMetadata ConvertNode() {
			ExpressionMetadata nestedMetadata = null;
			if (_node is UnaryExpression unaryExpressionNode) {
				nestedMetadata = ConvertNode(unaryExpressionNode.Operand, modelMetadata);
			}

			if (nestedMetadata != null) {
				return ApplyNot(nestedMetadata);
			} else {
				throw new NotImplementedException();
			}
		}

		internal static ExpressionMetadata ApplyNot(ExpressionMetadata metadata) {
			if (metadata.NodeType == ExpressionMetadataNodeType.Comparison) {
				return ApplyComparisonNot(metadata);
			}
			if (metadata.NodeType == ExpressionMetadataNodeType.Property) {
				return ApplyPropertyNot(metadata);
			}
			throw new NotImplementedException();
		}

		private static ExpressionMetadata ApplyPropertyNot(ExpressionMetadata metadata) {
			if (metadata.Parameter.Type == typeof(bool)) {
				metadata.Parameter.Value = !(bool)metadata.Parameter.Value;
			} else {
				throw new NotImplementedException();
			}

			return metadata;
		}

		private static ExpressionMetadata ApplyComparisonNot(ExpressionMetadata metadata) {
			metadata.ComparisonType = GetNotForComparisonType(metadata.ComparisonType);
			return metadata;
		}

		private static FilterComparisonType GetNotForComparisonType(FilterComparisonType comparisonType) {
			if (NotType.ContainsKey(comparisonType)) {
				return NotType[comparisonType];
			}
			throw new NotImplementedException();
		}
	}
}
