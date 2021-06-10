using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ATF.Repository.Exceptions;
using ATF.Repository.Mapping;
using Terrasoft.Core.Entities;

namespace ATF.Repository.ExpressionConverters
{
	internal abstract class ExpressionConverter
	{
		private static Dictionary<Type, ExpressionConverter> _expressionConverters = new Dictionary<Type, ExpressionConverter>();
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

		internal abstract ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata);

		internal static ExpressionMetadata ConvertModelQueryExpression(Expression expression, ExpressionModelMetadata modelMetadata) {
			ExpressionConverter expressionConverter = null;
			if (expression.NodeType == ExpressionType.Call) {
				expressionConverter = GetExpressionConverter<MethodCallExpressionConverter>();
			}
			if (expression.NodeType == ExpressionType.Lambda) {
				expressionConverter = GetExpressionConverter<LambdaExpressionConverter>();
			}
			if (expression.NodeType == ExpressionType.MemberAccess) {
				expressionConverter = GetExpressionConverter<MemberAccessExpressionConverter>();
			}
			if (expression.NodeType == ExpressionType.Constant) {
				expressionConverter = GetExpressionConverter<ConstantExpressionConverter>();
			}
			if (expression.NodeType == ExpressionType.Convert) {
				expressionConverter = GetExpressionConverter<ConvertExpressionConverter>();
			}
			if (expression.NodeType == ExpressionType.Not) {
				expressionConverter = GetExpressionConverter<NotExpressionConverter>();
			}

			if (expressionConverter == null) {
				throw new NotImplementedException();
			}

			return expressionConverter.Convert(expression, modelMetadata);
		}

		protected static ExpressionConverter GetExpressionConverter<T>() where T: ExpressionConverter{
			var type = typeof(T);
			return GetExpressionConverter(type);
		}

		protected static ExpressionConverter GetExpressionConverter(Type type) {
			if (!_expressionConverters.ContainsKey(type)) {
				_expressionConverters[type] = CreateExpressionConverter(type);
			}

			return _expressionConverters[type];
		}

		private static ExpressionConverter CreateExpressionConverter(Type type) {
			return Activator.CreateInstance(type) as ExpressionConverter;
		}

		private static object DynamicInvoke(Expression node) {
			return Expression.Lambda(node).Compile().DynamicInvoke();
		}

		protected static bool TryDynamicInvoke(Expression node, out object value) {
			var invoked = true;
			value = null;
			try {
				value = DynamicInvoke(node);
			} catch (Exception) {
				invoked = false;
			}

			return invoked;
		}

		protected static ExpressionMetadata GetPropertyMetadata(Expression expression, object value) {
			return GetPropertyMetadata(expression.Type, value);
		}

		protected static ExpressionMetadata GetPropertyMetadata(Type type, object value) {
			return new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Property,
				Parameter = new ExpressionMetadataParameter() {
					Type = type,
					Value = value
				}
			};
		}

		protected static bool IsColumnPathMember(Expression node, ExpressionModelMetadata modelMetadata) {
			if (node is MemberExpression memberNode) {
				if (memberNode.Member.DeclaringType == null || !(memberNode.Member.DeclaringType == typeof(BaseModel) || memberNode.Member.DeclaringType.IsSubclassOf(typeof(BaseModel)))) {
					return false;
				}
				return IsColumnPathMember(memberNode.Expression, modelMetadata);
			}

			if (node is ParameterExpression parameterExpression) {
				return parameterExpression.Name == modelMetadata.Name && parameterExpression.Type == modelMetadata.Type;
			}
			return false;
		}

		protected static bool IsDetailPathMember(Expression node, ExpressionModelMetadata modelMetadata) {
			if (node is MethodCallExpression methodCallExpression) {
				if (methodCallExpression.Arguments.Count > 0) {
					return IsDetailPathMember(methodCallExpression.Arguments.First(), modelMetadata);
				}
				return false;
			}

			if (node is MemberExpression memberNode && memberNode.Member.DeclaringType != null &&
			    (memberNode.Member.DeclaringType == typeof(BaseModel) ||
			     memberNode.Member.DeclaringType.IsSubclassOf(typeof(BaseModel))) &&
			    IsColumnPathMember(memberNode.Expression, modelMetadata)) {
				return IsDetailMember(memberNode.Member);
			}

			return false;
		}

		private static bool IsDetailMember(MemberInfo memberInfo) {
			var details = ModelMapper.GetDetails(memberInfo.DeclaringType);
			return details.Any(x => x.PropertyName == memberInfo.Name);
		}

		protected static ExpressionMetadata GetColumnPathMetadata(Expression node, ExpressionModelMetadata modelMetadata) {
			return new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Column,
				Parameter = new ExpressionMetadataParameter() {
					Type = node.Type,
					ColumnPath = GetColumnPath(node, modelMetadata)
				}
			};
		}

		private static string GetColumnPath(Expression node, ExpressionModelMetadata modelMetadata) {
			var chainList = GetColumnPathChainList(node);
			return ModelUtilities.GetColumnPath(modelMetadata.Type, chainList);
		}

		private static List<string> GetColumnPathChainList(Expression node, List<string> chainList = null) {
			chainList = chainList ?? new List<string>();
			if (node is MemberExpression memberNode) {
				chainList = GetColumnPathChainList(memberNode.Expression, chainList);
				chainList.Add(memberNode.Member.Name);
			}
			return chainList;
		}

		protected static Expression GetExpressionBody(Expression expression) {
			if (expression is MethodCallExpression methodCallExpression) {
				if (methodCallExpression.Arguments.Count < 2) {
					throw new ExpressionConvertException();
				}
				expression = methodCallExpression.Arguments.Skip(1).First();
			}

			if (!(expression is UnaryExpression unaryExpression)) {
				throw new ExpressionConvertException();
			}
			if (!(unaryExpression.Operand is LambdaExpression lambdaExpression)) {
				throw new ExpressionConvertException();
			}

			return lambdaExpression.Body;
		}

		protected static ExpressionMetadata ApplyNot(ExpressionMetadata expressionMetadata) {
			if (expressionMetadata.NodeType == ExpressionMetadataNodeType.Comparison && NotType.ContainsKey(expressionMetadata.ComparisonType)) {
				expressionMetadata.ComparisonType = NotType[expressionMetadata.ComparisonType];
			}
			return expressionMetadata;
		}
	}
}
