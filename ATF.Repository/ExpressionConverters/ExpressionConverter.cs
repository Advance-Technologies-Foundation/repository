using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ATF.Repository.Mapping;

namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Linq.Expressions;

	internal abstract class ExpressionConverter
	{
		internal ExpressionModelMetadata modelMetadata;
		internal abstract ExpressionMetadata ConvertNode();

		protected static ExpressionMetadata ConvertNode(Expression node, ExpressionModelMetadata modelMetadata) {
			ExpressionMetadata metadata = null;
			ExpressionConverter nodeConverter = null;
			switch (node.NodeType) {
				case ExpressionType.Call:
					nodeConverter = new MethodCallExpressionConverter((MethodCallExpression)node) { modelMetadata = modelMetadata};
					break;
				case ExpressionType.Quote:
					nodeConverter = new QuoteExpressionConverter(node) { modelMetadata = modelMetadata};
					break;
				case ExpressionType.Lambda:
					nodeConverter = new LambdaConverter((LambdaExpression) node) { modelMetadata = modelMetadata};
					break;
				case ExpressionType.Equal:
				case ExpressionType.NotEqual:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
					nodeConverter = new ComparisonConverter((BinaryExpression) node) { modelMetadata = modelMetadata};
					break;
				case ExpressionType.MemberAccess:
					nodeConverter = new MemberAccessConverter((MemberExpression) node) { modelMetadata = modelMetadata};
					break;
				case ExpressionType.Convert:
					nodeConverter = new DataTypeConvertConverter((UnaryExpression) node) { modelMetadata = modelMetadata};
					break;
				case ExpressionType.Constant:
					nodeConverter = new ConstantConverter((ConstantExpression) node) { modelMetadata = modelMetadata};
					break;
				case ExpressionType.And:
				case ExpressionType.AndAlso:
				case ExpressionType.Or:
				case ExpressionType.OrElse:
					nodeConverter = new GroupExpressionConverter((BinaryExpression) node) { modelMetadata = modelMetadata};
					break;
				case ExpressionType.Not:
					nodeConverter = new NotExpressionConverter(node) { modelMetadata = modelMetadata};
					break;
				case ExpressionType.New:
					nodeConverter = new NewConverter(node) { modelMetadata = modelMetadata};
					break;
				default:
					throw new NotImplementedException();
			}

			metadata = nodeConverter?.ConvertNode() ?? null;
			return metadata;
		}

		private object DynamicInvoke(Expression node) {
			return Expression.Lambda(node).Compile().DynamicInvoke();
		}

		protected bool TryDynamicInvoke(Expression node, out object value) {
			var invoked = true;
			value = null;
			try {
				value = DynamicInvoke(node);
			} catch (Exception) {
				invoked = false;
			}

			return invoked;
		}

		protected ExpressionMetadata GetPropertyMetadata(Expression node, object value) {
			return new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Property,
				Parameter = new ExpressionMetadataParameter() {
					Type = node.Type,
					Value = value
				}
			};
		}

		protected bool IsColumnPathMember(Expression node) {
			if (node is MemberExpression memberNode) {
				if (memberNode.Member.DeclaringType == null || !(memberNode.Member.DeclaringType == typeof(BaseModel) || memberNode.Member.DeclaringType.IsSubclassOf(typeof(BaseModel)))) {
					return false;
				}
				return IsColumnPathMember(memberNode.Expression);
			}

			if (node is ParameterExpression parameterExpression) {
				return parameterExpression.Name == modelMetadata.Name && parameterExpression.Type == modelMetadata.Type;
			}
			return false;
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

		protected ExpressionMetadata GetColumnPathMetadata(Expression node) {
			return new ExpressionMetadata() {
				NodeType = ExpressionMetadataNodeType.Column,
				Parameter = new ExpressionMetadataParameter() {
					Type = node.Type,
					ColumnPath = GetColumnPath(node)
				}
			};
		}

		private string GetColumnPath(Expression node) {
			var chainList = GetColumnPathChainList(node);
			return ModelUtilities.GetColumnPath(modelMetadata.Type, chainList);
		}

		private List<string> GetColumnPathChainList(Expression node, List<string> chainList = null) {
			chainList = chainList ?? new List<string>();
			if (node is MemberExpression memberNode) {
				chainList = GetColumnPathChainList(memberNode.Expression, chainList);
				chainList.Add(memberNode.Member.Name);
			}

			return chainList;
		}
	}
}
