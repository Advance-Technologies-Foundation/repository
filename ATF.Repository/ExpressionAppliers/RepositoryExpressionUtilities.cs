namespace ATF.Repository.ExpressionAppliers
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;

	internal static class RepositoryExpressionUtilities
	{
		private const string AnyMethodName = "Any";
		private static readonly List<string> AggregationMethods = new List<string>() {"Average", "Count", "Max", "Min", "Sum"};

		private static object DynamicInvoke(Expression node) {
			return Expression.Lambda(node).Compile().DynamicInvoke();
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

		internal static bool IsAggregationMethodExpression(Expression expression) {
			var methodName = GetMethodName(expression);
			return AggregationMethods.Contains(methodName);
		}
		internal static bool IsAnyMethodExpression(Expression expression) {
			var methodName = GetMethodName(expression);
			return methodName == AnyMethodName;
		}


		internal static string GetMethodName(Expression expression) {
			if (!(expression is MethodCallExpression methodCallExpression)) {
				return null;
			}

			return methodCallExpression.Method.Name;
		}

		internal static string GetAggregationColumnName(string methodName) {
			return $"{methodName.ToUpper()}Value";
		}

		internal static string GetAnyColumnName() {
			return $"{AnyMethodName.ToUpper()}Value";
		}
	}
}
