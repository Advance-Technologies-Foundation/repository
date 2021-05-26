using System.Linq;

namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using Terrasoft.Core.Entities;

	internal class ComparisonConverter : ExpressionConverter
	{
		private readonly BinaryExpression _node;
		private static readonly Dictionary<ExpressionType, FilterComparisonType> ComparisonTypes = new Dictionary<ExpressionType, FilterComparisonType>() {
			{ExpressionType.Equal, FilterComparisonType.Equal},
			{ExpressionType.NotEqual, FilterComparisonType.NotEqual},
			{ExpressionType.GreaterThan, FilterComparisonType.Greater},
			{ExpressionType.GreaterThanOrEqual, FilterComparisonType.GreaterOrEqual},
			{ExpressionType.LessThan, FilterComparisonType.Less},
			{ExpressionType.LessThanOrEqual, FilterComparisonType.LessOrEqual}
		};

		public ComparisonConverter(BinaryExpression node) {
			_node = node;
		}

		internal override ExpressionMetadata ConvertNode() {
			FilterComparisonType comparisonType = GetComparisonType(_node.NodeType);
			return GenerateComparisonExpressionMetadata(_node.Left, comparisonType, _node.Right, modelMetadata);
		}

		private static bool CanBeConvertedAsIsNullComparison(ExpressionMetadata left, FilterComparisonType comparisonType, ExpressionMetadata right) {
			return right.NodeType == ExpressionMetadataNodeType.Property &&
			       right.Parameter.Value == null &&
			       left.NodeType == ExpressionMetadataNodeType.Column &&
			       (comparisonType == FilterComparisonType.Equal || comparisonType == FilterComparisonType.NotEqual);
		}

		private static FilterComparisonType GetComparisonType(ExpressionType nodeType) {
			if (ComparisonTypes.ContainsKey(nodeType)) {
				return ComparisonTypes[nodeType];
			}

			throw new NotImplementedException();
		}

		internal static ExpressionMetadata GenerateComparisonExpressionMetadata(Expression leftExpression, FilterComparisonType comparisonType,
			Expression rightExpression, ExpressionModelMetadata modelMetadata) {
			var left = GenerateFilterElementExpression(leftExpression, modelMetadata);
			var right = GenerateFilterElementExpression(rightExpression, modelMetadata);
			if (left == null || right == null) {
				return null;
			}

			return CanBeConvertedAsIsNullComparison(left, comparisonType, right)
				? GenerateIsNullComparisonExpressionMetadata(left, comparisonType)
				: GenerateComparisonExpressionMetadata(left, comparisonType, right);
		}

		internal static ExpressionMetadata GenerateComparisonExpressionMetadata(ExpressionMetadata left, FilterComparisonType comparisonType, params ExpressionMetadata[] rights) {
			return new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Comparison,
				LeftExpression = left,
				RightExpressions = rights.ToList(),
				ComparisonType = comparisonType
			};
		}

		private static ExpressionMetadata GenerateIsNullComparisonExpressionMetadata(ExpressionMetadata left, FilterComparisonType comparisonType) {
			var nullComparisonType = GetNullComparisonType(comparisonType);
			return new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Comparison,
				LeftExpression = left,
				ComparisonType = nullComparisonType
			};
		}

		private static FilterComparisonType GetNullComparisonType(FilterComparisonType comparisonType) {
			return comparisonType == FilterComparisonType.NotEqual
				? FilterComparisonType.IsNotNull
				: FilterComparisonType.IsNull;
		}

		private static ExpressionMetadata GenerateFilterElementExpression(Expression node,
			ExpressionModelMetadata modelMetadata) {
			if (IsDetailPathMember(node, modelMetadata)) {
			}
			return ConvertNode(node, modelMetadata);
		}
	}
}
