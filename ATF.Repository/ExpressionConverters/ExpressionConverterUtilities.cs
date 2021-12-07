using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ATF.Repository.Exceptions;
using ATF.Repository.ExpressionConverters;
using ATF.Repository.Mapping;
using Terrasoft.Common;
using Terrasoft.Core.Entities;

namespace ATF.Repository.ExpressionConverters
{
	internal static class ExpressionConverterUtilities
	{
		private static Type _baseModelType = typeof(BaseModel);
		private static Dictionary<ExpressionType, LogicalOperationStrict> _logicalOperations =
			new Dictionary<ExpressionType, LogicalOperationStrict>() {
				{ExpressionType.And, LogicalOperationStrict.And},
				{ExpressionType.AndAlso, LogicalOperationStrict.And},
				{ExpressionType.Or, LogicalOperationStrict.Or},
				{ExpressionType.OrElse, LogicalOperationStrict.Or}
			};

		private static Dictionary<ExpressionType, FilterComparisonType> _comparisonTypes =
			new Dictionary<ExpressionType, FilterComparisonType>() {
				{ExpressionType.Equal, FilterComparisonType.Equal},
				{ExpressionType.GreaterThan, FilterComparisonType.Greater},
				{ExpressionType.GreaterThanOrEqual, FilterComparisonType.GreaterOrEqual},
				{ExpressionType.LessThan, FilterComparisonType.Less},
				{ExpressionType.LessThanOrEqual, FilterComparisonType.LessOrEqual},
				{ExpressionType.NotEqual, FilterComparisonType.NotEqual}
			};
		private static Dictionary<FilterComparisonType, FilterComparisonType> _notTypes = new Dictionary<FilterComparisonType, FilterComparisonType>() {
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

		internal static FilterComparisonType GetNotComparisonType(FilterComparisonType filterComparisonType) {
			if (_notTypes.ContainsKey(filterComparisonType)) {
				return _notTypes[filterComparisonType];
			}
			throw new NotImplementedException( $"Cannot find Not comparisonType for {filterComparisonType}");
		}

		internal static bool TryDynamicInvoke(Expression node, out object value) {
			var invoked = true;
			value = null;
			try {
				value = DynamicInvoke(node);
			} catch (Exception) {
				invoked = false;
			}

			return invoked;
		}

		internal static ExpressionMetadata GetPropertyMetadata(Expression expression, object value) {
			return GetPropertyMetadata(expression.Type, value);
		}

		internal static ExpressionMetadata GetPropertyMetadata(Type type, object value) {
			return new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Property,
				Parameter = new ExpressionMetadataParameter() {
					Type = type,
					Value = value
				}
			};
		}

		private static object DynamicInvoke(Expression node) {
			return Expression.Lambda(node).Compile().DynamicInvoke();
		}

		internal static bool IsLogicalOperationExpressionType(ExpressionType expressionType) {
			return _logicalOperations.ContainsKey(expressionType);
		}

		internal static LogicalOperationStrict ConvertLogicalOperation(ExpressionType expressionType) {
			if (expressionType == ExpressionType.And || expressionType == ExpressionType.AndAlso) {
				return LogicalOperationStrict.And;
			}

			if (expressionType == ExpressionType.Or || expressionType == ExpressionType.OrElse) {
				return LogicalOperationStrict.Or;
			}
			throw new NotImplementedException( $"Not implemented convert LogicalOperation from {expressionType}");
		}

		internal static bool TryGetColumnMemberPath(Expression node, ExpressionModelMetadata modelMetadata, out string memberPath) {
			memberPath = GetColumnMemberPath(node, modelMetadata);
			return !string.IsNullOrEmpty(memberPath);
		}

		private static string GetColumnMemberPath(Expression node, ExpressionModelMetadata modelMetadata, List<string> pathParts = null) {
			pathParts = pathParts ?? new List<string>();
			if (node is MemberExpression memberExpression) {
				var member = memberExpression.Member;
				if (!IsModelType(member.DeclaringType) || !IsSchemaOrLookupProperty(member, out var columnPath)) {
					return string.Empty;
				}
				pathParts.Add(columnPath);
				return GetColumnMemberPath(memberExpression.Expression, modelMetadata, pathParts);
			}

			if (node is ParameterExpression parameterExpression && parameterExpression.Name == modelMetadata.Name &&
				parameterExpression.Type == modelMetadata.Type) {
				pathParts.Reverse();
				return string.Join(".", pathParts);
			}
			return string.Empty;
		}

		private static bool IsSchemaOrLookupProperty(MemberInfo member, out string columnPath) {
			columnPath = string.Empty;
			return IsSchemaProperty(member, out columnPath) || IsLookupProperty(member, out columnPath);
		}

		private static bool IsSchemaProperty(MemberInfo member, out string columnPath) {
			columnPath = string.Empty;
			var properties = ModelMapper.GetProperties(member.DeclaringType);
			var property = properties.FirstOrDefault(x => x.PropertyName == member.Name);
			if (property != null) {
				columnPath = property.EntityColumnName;
			}

			return !string.IsNullOrEmpty(columnPath);
		}

		private static bool IsLookupProperty(MemberInfo member, out string columnPath) {
			columnPath = string.Empty;
			var properties = ModelMapper.GetLookups(member.DeclaringType);
			var property = properties.FirstOrDefault(x => x.PropertyName == member.Name);
			if (property != null) {
				columnPath = property.EntityColumnName;
			}

			return !string.IsNullOrEmpty(columnPath);
		}

		private static bool IsDetailProperty(MemberInfo member, out string columnPath) {
			columnPath = string.Empty;
			var properties = ModelMapper.GetDetails(member.DeclaringType);
			var property = properties.FirstOrDefault(x => x.PropertyName == member.Name);
			if (property != null) {
				columnPath = BuildDetailPath(property);
			}

			return !string.IsNullOrEmpty(columnPath);
		}

		private static string BuildDetailPath(ModelItem property) {
			var detailSchemaName = ModelUtilities.GetSchemaName(property.DataValueType);
			var detailModelProperties = ModelMapper.GetProperties(property.DataValueType);
			var detailLinkProperty =
				detailModelProperties.First(x => x.PropertyName == property.DetailLinkPropertyName);
			var path = new List<string>() {"["};
			path.Add(detailSchemaName);
			path.Add(":");
			path.Add(detailLinkProperty.EntityColumnName);
			if (!string.IsNullOrEmpty(property.MasterEntityColumnName) && property.MasterEntityColumnName != "Id") {
				path.Add(":");
				path.Add(property.MasterEntityColumnName);

			}
			path.Add("]");

			return string.Join("", path);
		}

		private static bool IsModelType(Type type) {
			return type != null && (type == _baseModelType || type.IsSubclassOf(_baseModelType));
		}

		public static ExpressionMetadata GetColumnMetadata(Type type, string columnPath) {
			return new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Column,
				Parameter = new ExpressionMetadataParameter() {
					Type = type,
					ColumnPath = columnPath
				}
			};
		}

		public static bool IsComparisonType(ExpressionType expressionNodeType) {
			return _comparisonTypes.ContainsKey(expressionNodeType);
		}

		public static FilterComparisonType GetComparisonType(ExpressionType expressionNodeType) {
			if (_comparisonTypes.ContainsKey(expressionNodeType)) {
				return _comparisonTypes[expressionNodeType];
			}

			throw new NotSupportedException($"Comparison type from {expressionNodeType} ExpressionType not supported");
		}

		public static Expression GetSecondArgumentExpression(MethodCallExpression expression) {
			if (expression.Arguments.Count < 2) {
				throw new ExpressionConvertException();
			}

			var aggregateBy = expression.Arguments.Skip(1).First();
			if (aggregateBy is UnaryExpression unaryExpression &&
				unaryExpression.Operand is LambdaExpression lambdaExpressionOperand) {
				return lambdaExpressionOperand.Body;
			}

			if (aggregateBy is LambdaExpression lambdaExpression) {
				return lambdaExpression.Body;
			}
			throw new ExpressionConvertException();
		}

		private class DetailPropertyParseData
		{
			public bool Success { get; set; }
			public string DetailPath { get; set; }
			public MemberExpression DetailMemberExpression { get; set; }
		}

		public static bool TryGetDetailProperty(Expression methodCallExpression, ExpressionModelMetadata modelMetadata, out string detailPath, out MemberExpression expression) {
			var parseData = GetDetailPropertyParseData(methodCallExpression, modelMetadata);
			detailPath = parseData?.DetailPath ?? string.Empty;
			expression = parseData?.DetailMemberExpression;
			return parseData?.Success ?? false;
		}

		private static DetailPropertyParseData GetDetailPropertyParseData(Expression expression,
			ExpressionModelMetadata modelMetadata, List<string> pathParts = null, MemberExpression detailMemberExpression = null) {
			if (expression is MethodCallExpression methodCallExpression && methodCallExpression.Arguments.Count > 0) {
				return GetDetailPropertyParseData(methodCallExpression.Arguments.First(), modelMetadata);
			}

			if (expression is MemberExpression memberExpression && IsModelType(memberExpression.Member.DeclaringType)) {
				if (IsDetailProperty(memberExpression.Member, out var memberColumnPath)) {
					return GetDetailPropertyParseData(memberExpression.Expression, modelMetadata,
						new List<string>() {memberColumnPath}, memberExpression);
				}

				if (IsSchemaOrLookupProperty(memberExpression.Member, out var pathPart)) {
					pathParts?.Add(pathPart);
					return GetDetailPropertyParseData(memberExpression.Expression, modelMetadata,
						pathParts, detailMemberExpression);
				}
			}
			if (expression is ParameterExpression parameterExpression && parameterExpression.Name == modelMetadata.Name &&
				parameterExpression.Type == modelMetadata.Type) {
				pathParts?.Reverse();
				return new DetailPropertyParseData() {
					Success = true,
					DetailPath = string.Join(".", pathParts ?? new List<string>()),
					DetailMemberExpression = detailMemberExpression
				};
			}
			return null;
		}

	}
}
