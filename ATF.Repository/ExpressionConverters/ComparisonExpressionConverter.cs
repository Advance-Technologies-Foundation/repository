using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ATF.Repository.Exceptions;
using Terrasoft.Common;
using Terrasoft.Core.Entities;

namespace ATF.Repository.ExpressionConverters
{
	internal class ComparisonExpressionConverter: ExpressionConverter {

		private static readonly Dictionary<ExpressionType, FilterComparisonType> ComparisonTypes = new Dictionary<ExpressionType, FilterComparisonType>() {
			{ExpressionType.Equal, FilterComparisonType.Equal},
			{ExpressionType.NotEqual, FilterComparisonType.NotEqual},
			{ExpressionType.GreaterThan, FilterComparisonType.Greater},
			{ExpressionType.GreaterThanOrEqual, FilterComparisonType.GreaterOrEqual},
			{ExpressionType.LessThan, FilterComparisonType.Less},
			{ExpressionType.LessThanOrEqual, FilterComparisonType.LessOrEqual}
		};

		private static readonly List<ExpressionType> GroupOperations = new List<ExpressionType>() { ExpressionType.Add, ExpressionType.AndAlso, ExpressionType.Or, ExpressionType.OrElse};

		internal override ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			if (!(expression is MethodCallExpression methodCallExpression) || methodCallExpression.Arguments.Count != 2) {
				throw new ExpressionConvertException();
			}

			var argumentExpression = methodCallExpression.Arguments.Skip(1).First();
			if (!(argumentExpression is UnaryExpression unaryExpression)) {
				throw new ExpressionConvertException();
			}
			if (!(unaryExpression.Operand is LambdaExpression lambdaExpression)) {
				throw new ExpressionConvertException();
			}

			var innerExpression = lambdaExpression.Body;
			return ConvertExpression(innerExpression, modelMetadata);
		}


		private static ExpressionMetadata ConvertExpressionGroup(BinaryExpression binaryExpression, ExpressionModelMetadata modelMetadata) {
			var logicalOperation = GetLogicalOperation(binaryExpression.NodeType);
			var groupExpressionMetadata = GenerateGroup(logicalOperation);
			groupExpressionMetadata.Items.Add(ConvertExpression(binaryExpression.Left, modelMetadata));
			groupExpressionMetadata.Items.Add(ConvertExpression(binaryExpression.Right, modelMetadata));
			return groupExpressionMetadata;
		}

		private static LogicalOperationStrict GetLogicalOperation(ExpressionType expressionType) {
			return  expressionType == ExpressionType.Add || expressionType == ExpressionType.AndAlso
				? LogicalOperationStrict.And
				: LogicalOperationStrict.Or;
		}

		protected static ExpressionMetadata GenerateGroup(LogicalOperationStrict logicalOperation) {
			return new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Group,
				LogicalOperation = logicalOperation,
				Items = new List<ExpressionMetadata>()
			};
		}

		private static ExpressionMetadata ConvertExpression(Expression expression, ExpressionModelMetadata modelMetadata) {
			if (expression is BinaryExpression binaryExpression) {
				return ConvertBinaryExpressionMetadata(binaryExpression, modelMetadata);
			}
			if (IsDetailPathMember(expression, modelMetadata)) {
				throw new NotSupportedException();
			}
			if (expression is MethodCallExpression methodCallExpression) {
				var converter = GetExpressionConverter(typeof(MethodCallExpressionConverter));
				return converter.Convert(methodCallExpression, modelMetadata);
			}
			if (expression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Not) {
				var response = ConvertExpression(unaryExpression.Operand, modelMetadata);
				return ApplyNot(response);
			}
			return ConvertUnaryExpressionMetadata(expression, modelMetadata);
		}



		private static ExpressionMetadata ConvertUnaryExpressionMetadata(Expression left, ExpressionModelMetadata modelMetadata) {
			var leftExpressionMetadata = GenerateFilterElementExpression(left, modelMetadata);
			var rightExpressionMetadata = GetPropertyMetadata(typeof(bool), true);
			var comparisonType = leftExpressionMetadata.IsNot
				? FilterComparisonType.NotEqual
				: FilterComparisonType.Equal;
			return GenerateComparisonExpressionMetadata(leftExpressionMetadata, comparisonType,
				rightExpressionMetadata);
		}

		private static ExpressionMetadata ConvertBinaryExpressionMetadata(BinaryExpression binaryExpression, ExpressionModelMetadata modelMetadata) {
			var nodeType = binaryExpression.NodeType;
			return GroupOperations.Contains(binaryExpression.NodeType)
				? ConvertExpressionGroup(binaryExpression, modelMetadata)
				: CreateBinaryComparisonExpressionMetadata(binaryExpression, modelMetadata);
		}

		private static ExpressionMetadata CreateBinaryComparisonExpressionMetadata(BinaryExpression binaryExpression, ExpressionModelMetadata modelMetadata) {
			if (GroupOperations.Contains(binaryExpression.NodeType)) {
				return ConvertExpressionGroup(binaryExpression, modelMetadata);
			}
			var comparisonType = GetComparisonType(binaryExpression.NodeType);
			var leftExpressionMetadata = GenerateFilterElementExpression(binaryExpression.Left, modelMetadata);
			var rightExpressionMetadata = GenerateFilterElementExpression(binaryExpression.Right, modelMetadata);
			return CanBeConvertedAsIsNullComparison(leftExpressionMetadata, comparisonType, rightExpressionMetadata)
				? GenerateIsNullExpressionMetadata(leftExpressionMetadata, comparisonType)
				: GenerateComparisonExpressionMetadata(leftExpressionMetadata, comparisonType,
				rightExpressionMetadata);
		}

		private static ExpressionMetadata GenerateIsNullExpressionMetadata(ExpressionMetadata left,
			FilterComparisonType comparisonType) {
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
			throw new NotSupportedException();
		}

		internal static ExpressionMetadata GenerateComparisonExpressionMetadata(Expression left,
			FilterComparisonType comparisonType, ExpressionModelMetadata modelMetadata, params Expression[] rights) {
			var leftExpressionMetadata = GenerateFilterElementExpression(left, modelMetadata);
			var rightExpressionMetadataList =
				rights.Select(right => GenerateFilterElementExpression(right, modelMetadata)).ToArray();
			return GenerateComparisonExpressionMetadata(leftExpressionMetadata, comparisonType,
				rightExpressionMetadataList);
		}

		internal static ExpressionMetadata GenerateComparisonExpressionMetadata(ExpressionMetadata left, FilterComparisonType comparisonType, params ExpressionMetadata[] rights) {
			return new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Comparison,
				LeftExpression = left,
				RightExpressions = rights.ToList(),
				ComparisonType = comparisonType
			};
		}

		internal static ExpressionMetadata GenerateFilterElementExpression(Expression expression,
			ExpressionModelMetadata modelMetadata) {
			if (TryDynamicInvoke(expression, out var value)) {
				return GetPropertyMetadata(expression, value);
			}
			if (IsDetailPathMember(expression, modelMetadata)) {
				return ExtractDetailElementExpression(expression, modelMetadata);
			}

			return ConvertModelQueryExpression(expression, modelMetadata);
		}

		private static ExpressionMetadata ExtractDetailElementExpression(Expression expression,
			ExpressionModelMetadata modelMetadata) {
			var detailModelMetadata = ExpressionToMetadataConverter.CreateDetailExpressionModelMetadata(expression, modelMetadata);

			//var startedChain = ExpressionToMetadataConverter.ConvertExpressionToChain(expression);
			//var chain = ExpressionToMetadataConverter.Convert(expression, startedChain.Items.First().InputDtoType.Type);

			throw new NotImplementedException();
		}





	}
}
