using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ATF.Repository.Exceptions;

namespace ATF.Repository.ExpressionConverters
{
	internal class MethodCallExpressionConverter: ExpressionConverter {

		class MethodConverterItems
		{
			public List<string> MethodNames { get; set; }
			public Type MethodType { get; set; }
		}

		private static readonly List<MethodConverterItems>  ExpressionMethodConverters = new List<MethodConverterItems>() {
			new MethodConverterItems() {
				MethodNames = new List<string>() {"Where"},
				MethodType = typeof(ComparisonExpressionConverter)
			},
			new MethodConverterItems() {
				MethodNames = new List<string>() {"Any", "Count", "First", "FirstOrDefault"},
				MethodType = typeof(VariableComparisonExpressionConverter)
			},

			new MethodConverterItems() {
				MethodNames = new List<string>() {"Take"},
				MethodType = typeof(SkipTakeExpressionConverter)
			},
			new MethodConverterItems() {
				MethodNames = new List<string>() {"Skip"},
				MethodType = typeof(SkipTakeExpressionConverter)
			},
			new MethodConverterItems() {
				MethodNames = new List<string>() {"OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending"},
				MethodType = typeof(OrderMethodConverter)
			},
			new MethodConverterItems() {
				MethodNames = new List<string>() {"Min", "Max", "Average", "Sum"},
				MethodType = typeof(SimpleAggregationMethodConverter)
			},

		};

		private static readonly List<MethodConverterItems>  PropertyMethodConverters = new List<MethodConverterItems>() {
			new MethodConverterItems() {
				MethodNames = new List<string>() {"StartsWith"},
				MethodType = typeof(StringStartWithPropertyMethodConverter)
			},
			new MethodConverterItems() {
				MethodNames = new List<string>() {"EndsWith"},
				MethodType = typeof(StringEndWithPropertyMethodConverter)
			},
			new MethodConverterItems() {
				MethodNames = new List<string>() {"Contains"},
				MethodType = typeof(StringContainsPropertyMethodConverter)
			}
		};

		private static readonly List<MethodConverterItems>  FieldMethodConverters = new List<MethodConverterItems>() {
			new MethodConverterItems() {
				MethodNames = new List<string>() {"Contains"},
				MethodType = typeof(FieldContainsPropertyMethodConverter)
			}
		};

		internal override ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			if (!(expression is MethodCallExpression methodCallExpression)) {
				throw new ExpressionConvertException();
			}

			if (methodCallExpression.Object == null &&
			    TryGetExpressionMethodConverter(methodCallExpression, out var expressionMethodConverter)) {
				return expressionMethodConverter.Convert(methodCallExpression, modelMetadata);
			}

			if (methodCallExpression.Object != null && IsColumnPathMember(methodCallExpression.Object, modelMetadata) &&
			    TryGetPropertyMethodConverter(methodCallExpression, out var propertyMethodConverter)) {
				return propertyMethodConverter.Convert(methodCallExpression, modelMetadata);
			}

			if (methodCallExpression.Object != null && !IsColumnPathMember(methodCallExpression.Object, modelMetadata) &&
			    TryGetFieldMethodConverter(methodCallExpression, out var fieldMethodConverter)) {
				return fieldMethodConverter.Convert(methodCallExpression, modelMetadata);
			}

			if (TryDynamicInvoke(expression, out var value)) {
				return GetPropertyMetadata(expression, value);
			}

			throw new ExpressionConvertException();
		}

		private bool TryGetPropertyMethodConverter(MethodCallExpression methodCallExpression, out ExpressionConverter converter) {
			converter = null;
			var item = PropertyMethodConverters.FirstOrDefault(x => x.MethodNames.Contains(methodCallExpression.Method.Name));
			return item != null && (converter = GetExpressionConverter(item.MethodType)) != null;
		}

		private static bool TryGetExpressionMethodConverter(MethodCallExpression methodCallExpression,
			out ExpressionConverter converter) {
			converter = null;
			var item = ExpressionMethodConverters.FirstOrDefault(x => x.MethodNames.Contains(methodCallExpression.Method.Name));
			return item != null && (converter = GetExpressionConverter(item.MethodType)) != null;
		}

		private static bool TryGetFieldMethodConverter(MethodCallExpression methodCallExpression,
			out ExpressionConverter converter) {
			converter = null;
			var item = FieldMethodConverters.FirstOrDefault(x => x.MethodNames.Contains(methodCallExpression.Method.Name));
			return item != null && (converter = GetExpressionConverter(item.MethodType)) != null;
		}

	}
}
