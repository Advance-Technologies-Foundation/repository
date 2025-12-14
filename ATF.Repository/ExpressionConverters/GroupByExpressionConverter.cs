namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;

	internal static class GroupByExpressionConverter
	{
		internal static ExpressionMetadata Convert(MethodCallExpression expression, ExpressionModelMetadata modelMetadata) {
			if (expression.Arguments.Count < 3) {
				throw new NotSupportedException("GroupBy requires both keySelector and resultSelector");
			}

			var keys = ParseGroupByKeys(expression.Arguments[1], modelMetadata);
			var resultColumns = ParseResultSelector(expression.Arguments[2], modelMetadata, keys);

			var metadata = new ExpressionMetadata {
				MethodName = ConvertableExpressionMethod.GroupBy,
				NodeType = ExpressionMetadataNodeType.Group,
				Items = resultColumns
			};

			return metadata;
		}

		private static List<ExpressionMetadata> ParseResultSelector(Expression expression, ExpressionModelMetadata modelMetadata, List<ExpressionMetadata> groupKeys) {
			var resultColumns = new List<ExpressionMetadata>();

			Expression body;
			ParameterExpression groupByParam;
			ParameterExpression itemsParam;
			LambdaExpression lambdaExpr = null;

			if (expression is UnaryExpression unaryExpression && unaryExpression.Operand is LambdaExpression operandLambdaExpression) {
				body = operandLambdaExpression.Body;
				groupByParam = operandLambdaExpression.Parameters[0];
				itemsParam = operandLambdaExpression.Parameters[1];
				lambdaExpr = operandLambdaExpression;
			} else if (expression is LambdaExpression lambdaExpression) {
				body = lambdaExpression.Body;
				groupByParam = lambdaExpression.Parameters[0];
				itemsParam = lambdaExpression.Parameters[1];
				lambdaExpr = lambdaExpression;
			} else {
				throw new NotSupportedException("GroupBy resultSelector must be a lambda expression");
			}

			if (body is NewExpression newExpression) {
				for (var i = 0; i < newExpression.Arguments.Count; i++) {
					var argument = newExpression.Arguments[i];
					var memberName = newExpression.Members?[i]?.Name ?? $"Item{i + 1}";

					var rootParam = GetRootParameter(argument);

					if (rootParam == groupByParam) {
						var keyMetadata = ParseGroupByMember(argument, memberName, modelMetadata, groupKeys);
						if (keyMetadata != null) {
							resultColumns.Add(keyMetadata);
						}
					} else if (rootParam == itemsParam) {
						var itemsType = itemsParam.Type.GetGenericArguments().FirstOrDefault();
						var itemsModelMetadata = itemsType != null
							? new ExpressionModelMetadata { Type = itemsType, Name = itemsParam.Name }
							: modelMetadata;

						var aggregationMetadata = ParseAggregation(argument, memberName, itemsModelMetadata);
						if (aggregationMetadata != null) {
							resultColumns.Add(aggregationMetadata);
						}
					} else {
						throw new NotSupportedException($"GroupBy result selector contains unsupported expression type");
					}
				}
			} else {
				throw new NotSupportedException("GroupBy resultSelector body must be a NewExpression (anonymous type)");
			}

			return resultColumns;
		}

		private static ParameterExpression GetRootParameter(Expression expression) {
			while (expression != null) {
				if (expression is ParameterExpression paramExpr) {
					return paramExpr;
				} else if (expression is MemberExpression memberExpr) {
					expression = memberExpr.Expression;
				} else if (expression is MethodCallExpression methodExpr) {
					if (methodExpr.Object != null) {
						expression = methodExpr.Object;
					} else if (methodExpr.Arguments.Count > 0) {
						expression = methodExpr.Arguments[0];
					} else {
						return null;
					}
				} else if (expression is UnaryExpression unaryExpr) {
					expression = unaryExpr.Operand;
				} else {
					return null;
				}
			}
			return null;
		}

		private static ExpressionMetadata ParseGroupByMember(Expression expression, string alias,
			ExpressionModelMetadata modelMetadata, List<ExpressionMetadata> groupKeys) {

			if (expression is MemberExpression memberExpression) {
				var memberName = memberExpression.Member.Name;

				var keyMetadata = groupKeys.FirstOrDefault(k => k.Code == memberName);
				if (keyMetadata != null) {
					return new ExpressionMetadata {
						NodeType = keyMetadata.NodeType,
						Code = alias,
						Parameter = keyMetadata.Parameter,
						MethodName = keyMetadata.MethodName,
						DatePart = keyMetadata.DatePart
					};
				}
			}

			throw new NotSupportedException($"GroupBy result member {alias} does not match any grouping key");
		}

		private static ExpressionMetadata ParseAggregation(Expression expression, string alias, ExpressionModelMetadata modelMetadata) {
			if (expression is MethodCallExpression methodCallExpression) {
				var methodName = methodCallExpression.Method.Name;

				if (methodName == ConvertableExpressionMethod.Count ||
				    methodName == ConvertableExpressionMethod.Sum ||
				    methodName == ConvertableExpressionMethod.Max ||
				    methodName == ConvertableExpressionMethod.Min ||
				    methodName == ConvertableExpressionMethod.Average) {

					string columnPath = Constants.DefaultPrimiaryColumnName;

					if (methodCallExpression.Arguments.Count > 1) {
						var selectorExpression = ExpressionConverterUtilities.GetSecondArgumentExpression(methodCallExpression);
						if (selectorExpression is MemberExpression selectorMember) {
							// For simple member expressions like "y => y.DecimalValue", just get the member name
							columnPath = selectorMember.Member.Name;
						}
					}

					return new ExpressionMetadata {
						NodeType = ExpressionMetadataNodeType.Function,
						Code = alias,
						MethodName = methodName,
						Parameter = new ExpressionMetadataParameter {
							Type = methodCallExpression.Type,
							ColumnPath = columnPath
						}
					};
				}

				throw new NotSupportedException($"Aggregation method {methodName} is not supported in GroupBy");
			}

			throw new NotSupportedException("GroupBy aggregation must be a method call expression");
		}

		private static List<ExpressionMetadata> ParseGroupByKeys(Expression expression, ExpressionModelMetadata modelMetadata) {
			var groupKeys = new List<ExpressionMetadata>();

			Expression body;
			if (expression is UnaryExpression unaryExpression && unaryExpression.Operand is LambdaExpression operandLambdaExpression) {
				body = operandLambdaExpression.Body;
			} else if (expression is LambdaExpression lambdaExpression) {
				body = lambdaExpression.Body;
			} else {
				throw new NotSupportedException("GroupBy keySelector must be a lambda expression");
			}

			if (body is NewExpression newExpression) {
				for (var i = 0; i < newExpression.Arguments.Count; i++) {
					if (newExpression.Arguments[i].NodeType == ExpressionType.MemberAccess ||
					    newExpression.Arguments[i].NodeType == ExpressionType.Call) {
						var itemMember = ExpressionMetadataRawParser.Parse(newExpression.Arguments[i], modelMetadata);
						var itemExpressionMetadata = RawToExpressionMetadataConverter.Convert(itemMember, modelMetadata);
						itemExpressionMetadata.Code = newExpression.Members[i].Name;
						groupKeys.Add(itemExpressionMetadata);
					}
				}
			} else if (body is MemberExpression memberExpression) {
				var member = ExpressionMetadataRawParser.Parse(memberExpression, modelMetadata);
				var expressionMetadata = RawToExpressionMetadataConverter.Convert(member, modelMetadata);
				expressionMetadata.Code = memberExpression.Member.Name;
				groupKeys.Add(expressionMetadata);
			} else {
				throw new NotSupportedException("GroupBy keySelector must be either a NewExpression or MemberExpression");
			}

			return groupKeys;
		}
	}
}