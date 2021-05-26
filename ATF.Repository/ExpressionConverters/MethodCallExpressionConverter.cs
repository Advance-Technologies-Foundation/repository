using System.Collections.Generic;
using System.Linq;
using ATF.Repository.Exceptions;
using Terrasoft.Common;
using Terrasoft.Core.Entities;

namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Linq.Expressions;

	internal class MethodCallExpressionConverter : ExpressionConverter
	{

		private readonly MethodCallExpression _node;

		internal MethodCallExpressionConverter(MethodCallExpression node) {
			_node = node;
		}
		internal override ExpressionMetadata ConvertNode() {
			return TryDynamicInvoke(_node, out var value)
				? GetPropertyMetadata(_node, value)
				: ConvertNodeWithoutInvoke();
		}

		private ExpressionMetadata ConvertNodeWithoutInvoke() {
			if (_node?.Object == null) {
				if (TryConvertToDetailFilterExpression(_node)) {

				}
				throw new ExpressionConvertException();
			}
			if (IsColumnPathMember(_node.Object)) {
				return FunctionExpressionConverter.Convert(_node, modelMetadata);
			}
			if (TryConvertToFunctionExpression(_node, out ExpressionMetadata metadata)) {
				return metadata;
			}

			throw new ExpressionConvertException();
		}

		private bool TryConvertToDetailFilterExpression(MethodCallExpression node) {
			return false;
		}

		private bool TryConvertToFunctionExpression(MethodCallExpression node, out ExpressionMetadata metadata) {
			metadata = null;
			if (node.Method?.Name == "Contains" && node.Arguments.Any() && IsColumnPathMember(_node.Arguments.First())) {
				metadata = ConvertToContainsFunctionExpression(node);
				return metadata != null;
			}
			return false;
		}

		private ExpressionMetadata ConvertToContainsFunctionExpression(MethodCallExpression node) {
			var leftExpression = ConvertNode(_node.Arguments.First(), modelMetadata);
			if (!TryDynamicInvoke(_node.Object, out var collection)) {
				return null;
			}
			var method = RepositoryReflectionUtilities.GetGenericMethod(GetType(), "ConvertCollectionToObjectList",
				leftExpression.Parameter.Type);
			var list = (List<object>) method.Invoke(this, new object[] { collection });
			var rightExpressions = list.Select(value => {
				var property = GetPropertyMetadata(node, value);
				property.Parameter.Type = leftExpression.Parameter.Type;
				return property;
			}).ToArray();

			return ComparisonConverter.GenerateComparisonExpressionMetadata(leftExpression, FilterComparisonType.Equal,
				rightExpressions);
		}

		private List<object> ConvertCollectionToObjectList<T>(IList<T> original) {
			return original.Select(x => (object) x).ToList();
		}
	}
}
