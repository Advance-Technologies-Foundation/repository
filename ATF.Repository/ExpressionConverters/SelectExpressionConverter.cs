namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using ATF.Repository.Exceptions;

	internal static class SelectExpressionConverter
	{
		internal static ExpressionMetadata Convert(MethodCallExpression expression, ExpressionModelMetadata modelMetadata) {
			// DO NOT evaluate Select as constant - it's always a transformation
			// Get the selector lambda expression
			var selectorExpression = ExpressionConverterUtilities.GetSecondArgumentExpression(expression);

			// Parse the Select projection
			var metadata = new ExpressionMetadata {
				MethodName = ConvertableExpressionMethod.Select,
				NodeType = ExpressionMetadataNodeType.Group,
				Items = new List<ExpressionMetadata>()
			};

			// Handle different selector types
			if (selectorExpression is NewExpression newExpression) {
				// Select with anonymous type: x => new { x.Name, x.Email }
				ParseAnonymousTypeProjection(newExpression, modelMetadata, metadata);
			}
			else if (selectorExpression is MemberExpression memberExpression) {
				// Select single property: x => x.Name
				var item = ParseSingleProperty(memberExpression, modelMetadata);
				if (item != null) {
					metadata.Items.Add(item);
				}
			}
			else {
				throw new NotSupportedException($"Select expression type {selectorExpression.GetType().Name} is not supported");
			}

			return metadata;
		}

		private static void ParseAnonymousTypeProjection(NewExpression newExpression,
			ExpressionModelMetadata modelMetadata,
			ExpressionMetadata metadata) {

			// Parse each property in the anonymous type
			for (int i = 0; i < newExpression.Arguments.Count; i++) {
				var argument = newExpression.Arguments[i];
				var memberName = newExpression.Members?[i]?.Name ?? $"Item{i + 1}";

				var item = ParseProjectionItem(argument, memberName, modelMetadata);
				if (item != null) {
					metadata.Items.Add(item);
				}
			}
		}

		private static ExpressionMetadata ParseProjectionItem(Expression expression,
			string alias,
			ExpressionModelMetadata modelMetadata) {

			// 1. Check if it's a constant - store in metadata for client-side construction
			if (ExpressionConverterUtilities.TryDynamicInvoke(expression, out var constantValue)) {
				// Constants are not included in the query - store them in metadata to be used in result construction
				return new ExpressionMetadata {
					NodeType = ExpressionMetadataNodeType.Constant,
					Code = alias,
					Parameter = new ExpressionMetadataParameter {
						Value = constantValue
					}
				};
			}

			// 2. Check if it's a simple column reference: x.Name
			if (expression is MemberExpression memberExpression) {
				return ParseMemberExpression(memberExpression, alias, modelMetadata);
			}

			// 3. Check if it's a method call (aggregation on detail)
			if (expression is MethodCallExpression methodCallExpression) {
				return ParseMethodCallExpression(methodCallExpression, alias, modelMetadata);
			}

			throw new NotSupportedException($"Select projection item type {expression.GetType().Name} is not supported");
		}

		private static ExpressionMetadata ParseSingleProperty(MemberExpression memberExpression,
			ExpressionModelMetadata modelMetadata) {

			var memberName = memberExpression.Member.Name;
			return ParseMemberExpression(memberExpression, memberName, modelMetadata);
		}

		private static ExpressionMetadata ParseMemberExpression(MemberExpression memberExpression,
			string alias,
			ExpressionModelMetadata modelMetadata) {

			// 1. Try DatePart FIRST (x.DateTimeValue.Hour)
			if (ExpressionConverterUtilities.TryGetDatePartColumnMemberPath(memberExpression, modelMetadata,
				out var datePartColumnPath, out var datePart)) {
				return new ExpressionMetadata {
					NodeType = ExpressionMetadataNodeType.Function,
					Code = alias,
					DatePart = datePart,
					Parameter = new ExpressionMetadataParameter {
						Type = typeof(int), // DatePart always returns int
						ColumnPath = datePartColumnPath
					}
				};
			}

			// 2. Try regular column (x.Name or x.Account.Type)
			if (ExpressionConverterUtilities.TryGetColumnMemberPath(memberExpression, modelMetadata, out var columnPath)) {
				return new ExpressionMetadata {
					NodeType = ExpressionMetadataNodeType.Column,
					Code = alias,
					Parameter = new ExpressionMetadataParameter {
						Type = memberExpression.Type,
						ColumnPath = columnPath
					}
				};
			}

			throw new ExpressionConvertException();
		}

		private static ExpressionMetadata ParseMethodCallExpression(MethodCallExpression methodCallExpression,
			string alias,
			ExpressionModelMetadata modelMetadata) {

			var methodName = methodCallExpression.Method.Name;

			// Check if this is a detail aggregation: x.Details.Count()
			if (ExpressionConverterUtilities.TryGetDetailProperty(methodCallExpression, modelMetadata,
				out var detailPath, out var detailMemberExpression)) {

				return ParseDetailAggregation(methodCallExpression, detailPath, detailMemberExpression,
					alias, modelMetadata, methodName);
			}

			throw new NotSupportedException($"Method {methodName} is not supported in Select projection");
		}

		private static ExpressionMetadata ParseDetailAggregation(MethodCallExpression methodCallExpression,
			string detailPath,
			MemberExpression detailMemberExpression,
			string alias,
			ExpressionModelMetadata modelMetadata,
			string aggregationMethod) {

			// Build detail chain for the aggregation
			var detailChain = new ExpressionMetadataChain();

			// Parse detail expression chain
			// Examples:
			// 1. x.ContactCommunications.Count() - no Where
			// 2. x.ContactCommunications.Count(y => y.CommunicationTypeId == guid) - with predicate
			// 3. x.ContactCommunications.Where(y => ...).Count() - separate Where and Count

			ExpressionMetadata filterMetadata = null;

			// Check if Count/Sum/etc has a predicate argument
			if (methodCallExpression.Arguments.Count > 1) {
				// Count(y => y.Field == value)
				var predicateExpression = ExpressionConverterUtilities.GetSecondArgumentExpression(methodCallExpression);

				// Get detail model type from the detail member expression
				var detailModelType = detailMemberExpression.Type.GetGenericArguments().FirstOrDefault();
				if (detailModelType != null) {
					var detailModelMetadata = new ExpressionModelMetadata {
						Type = detailModelType,
						Name = ((LambdaExpression)methodCallExpression.Arguments[1]).Parameters[0].Name
					};

					// Convert the predicate to filter metadata
					filterMetadata = FilterConverter.Convert(predicateExpression, detailModelMetadata);
				}
			}
			else if (methodCallExpression.Arguments.Count > 0) {
				// Check if the argument is a Where call: x.Details.Where(y => ...).Count()
				var firstArg = methodCallExpression.Arguments[0];
				if (firstArg is MethodCallExpression whereExpression && whereExpression.Method.Name == "Where") {
					var predicateExpression = ExpressionConverterUtilities.GetSecondArgumentExpression(whereExpression);

					var detailModelType = detailMemberExpression.Type.GetGenericArguments().FirstOrDefault();
					if (detailModelType != null) {
						var detailModelMetadata = new ExpressionModelMetadata {
							Type = detailModelType,
							Name = ((LambdaExpression)whereExpression.Arguments[1]).Parameters[0].Name
						};

						filterMetadata = FilterConverter.Convert(predicateExpression, detailModelMetadata);
					}
				}
			}

			// Add filter to chain if exists
			if (filterMetadata != null) {
				var chainItem = new ExpressionMetadataChainItem(methodCallExpression) {
					ExpressionMetadata = filterMetadata
				};
				detailChain.Items.Add(chainItem);
			}

			return new ExpressionMetadata {
				NodeType = ExpressionMetadataNodeType.Detail,
				Code = alias,
				MethodName = aggregationMethod,
				DetailChain = detailChain,
				Parameter = new ExpressionMetadataParameter {
					Type = methodCallExpression.Type,
					ColumnPath = detailPath
				}
			};
		}
	}
}
