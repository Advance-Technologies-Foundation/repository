namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using ATF.Repository.Exceptions;

	internal static class ExpressionMetadataRawParser
	{
		internal static RawExpressionMetadata Parse(Expression expression, ExpressionModelMetadata modelMetadata) {
			if (expression is UnaryExpression unaryExpression) {
				return ConvertUnaryExpression(unaryExpression, modelMetadata);
			}

			if (expression is BinaryExpression binaryExpression) {
				return ConvertBinaryExpression(binaryExpression, modelMetadata);
			}

			if (expression is MemberExpression memberExpression) {
				return ConvertMemberExpression(memberExpression, modelMetadata);
			}

			if (expression is ConstantExpression constantExpression) {
				return ConvertConstantExpression(constantExpression, modelMetadata);
			}

			if (expression is NewExpression newExpression) {
				return ConvertConstantExpression(newExpression, modelMetadata);
			}

			if (expression is MethodCallExpression methodCallExpression) {
				return ConvertMethodCallExpression(methodCallExpression, modelMetadata);
			}

			if (expression is LambdaExpression lambdaExpression) {
				return Parse(lambdaExpression.Body, modelMetadata);
			}

			throw new NotImplementedException();
		}

		private static RawExpressionMetadata ConvertMethodCallExpression(MethodCallExpression methodCallExpression, ExpressionModelMetadata modelMetadata) {
			if (ExpressionConverterUtilities.TryDynamicInvoke(methodCallExpression, out var value)) {
				return CreateConstantRawExpressionMetadata(methodCallExpression.Type, value);
			}

			if (TryConvertToExternalCondition(methodCallExpression, modelMetadata,
				out var conditionRawMetadata)) {
				return conditionRawMetadata;
			}

			if (ExpressionConverterUtilities.TryGetDetailProperty(methodCallExpression, modelMetadata, out var detailPath, out var detailProperty)) {
				return new RawExpressionMetadata() {
					Type = ExpressionType.MemberAccess,
					Parameter = new ExpressionMetadataParameter() {
						Type = methodCallExpression.Type,
						ColumnPath = detailPath
					},
					RawDetailExpressionMetadata = new RawDetailExpressionMetadata() {
						CorePath = detailPath,
						DetailProperty = detailProperty,
						FullExpression =  methodCallExpression
					}
				};
			}

			throw new ExpressionConvertException();
		}

		private static bool TryConvertToExternalCondition(MethodCallExpression methodCallExpression, ExpressionModelMetadata modelMetadata, out RawExpressionMetadata rawExpressionMetadata) {
			rawExpressionMetadata = null;
			if (methodCallExpression.Object == null) {
				return false;
			}

			if (ExpressionConverterUtilities.TryGetColumnMemberPath(methodCallExpression.Object, modelMetadata,
				out var path)) {
				return TryConvertToExternalConditionAtColumn(methodCallExpression, modelMetadata, path, out rawExpressionMetadata);
			}
			return TryConvertToExternalConditionAtField(methodCallExpression, modelMetadata, out rawExpressionMetadata);

		}

		private static bool TryConvertToExternalConditionAtColumn(MethodCallExpression methodCallExpression, ExpressionModelMetadata modelMetadata, string columnPath, out RawExpressionMetadata rawExpressionMetadata) {
			rawExpressionMetadata = null;
			var expressionMethod = GetAvailableColumnMethodByMethodName(methodCallExpression.Method.Name);
			if (expressionMethod != AvailableColumnMethod.None) {
				return TryConvertToColumnContainsCondition(methodCallExpression, expressionMethod, columnPath, modelMetadata,
					out rawExpressionMetadata);
			}
			throw new ExpressionConvertException();
		}

		private static bool TryConvertToColumnContainsCondition(MethodCallExpression methodCallExpression,
			AvailableColumnMethod expressionMethod, string columnPath, ExpressionModelMetadata modelMetadata,
			out RawExpressionMetadata rawExpressionMetadata) {
			var left = CreateColumnRawExpressionMetadata(methodCallExpression.Object, columnPath);
			if (methodCallExpression.Arguments.Count != 1) {
				throw new ExpressionConvertException();
			}
			var rightExpression = methodCallExpression.Arguments.First();
			if (!ExpressionConverterUtilities.TryDynamicInvoke(rightExpression, out var value)) {
				throw new ExpressionConvertException();
			}

			var right = CreateConstantRawExpressionMetadata(rightExpression.Type, value);
			rawExpressionMetadata = new RawExpressionMetadata() {
				Type = ExpressionType.Equal,
				ColumnMethod = expressionMethod,
				Left = left,
				Right = right
			};
			return true;
		}

		private static AvailableColumnMethod GetAvailableColumnMethodByMethodName(string methodName) {
			switch (methodName) {
				case ConvertableExpressionMethod.Contains:
					return AvailableColumnMethod.Contains;
				case ConvertableExpressionMethod.StartsWith:
					return AvailableColumnMethod.StartWith;
				case ConvertableExpressionMethod.EndsWith:
					return AvailableColumnMethod.EndWith;
				default:
					return AvailableColumnMethod.None;
			}
		}



		private static bool TryConvertToExternalConditionAtField(MethodCallExpression methodCallExpression, ExpressionModelMetadata modelMetadata, out RawExpressionMetadata rawExpressionMetadata) {
			rawExpressionMetadata = null;
			if (methodCallExpression.Method.Name == ConvertableExpressionMethod.Contains) {
				return TryConvertToFieldContainsCondition(methodCallExpression, modelMetadata,
					out rawExpressionMetadata);
			}
			throw new ExpressionConvertException();

		}

		private static bool TryConvertToFieldContainsCondition(MethodCallExpression methodCallExpression, ExpressionModelMetadata modelMetadata, out RawExpressionMetadata rawExpressionMetadata) {
			if (methodCallExpression.Arguments.Count != 1) {
				throw new ExpressionConvertException();
			}

			var columnExpression = methodCallExpression.Arguments.First();
			if (!ExpressionConverterUtilities.TryGetColumnMemberPath(columnExpression, modelMetadata,
				out var columnPath)) {
				throw new ExpressionConvertException();
			}

			var left = CreateColumnRawExpressionMetadata(columnExpression, columnPath);

			var valueList = GetRightValuesList(methodCallExpression.Object, columnExpression.Type);
			rawExpressionMetadata = new RawExpressionMetadata() {
				Type = ExpressionType.Equal,
				FieldMethod = AvailableFieldMethod.In,
				Left = left
			};
			var rights = rawExpressionMetadata.Items;
			valueList.ForEach(x=>rights.Add(CreateConstantRawExpressionMetadata(columnExpression.Type, x)));
			return true;
		}

		private static List<object> GetRightValuesList(Expression collectionExpression, Type valueType) {
			if (!ExpressionConverterUtilities.TryDynamicInvoke(collectionExpression, out var collection)) {
				throw new ExpressionConvertException();
			}
			var method = RepositoryReflectionUtilities.GetGenericMethod(typeof(ExpressionMetadataRawParser), "ConvertCollectionToObjectList",
				valueType);
			var list = (List<object>) method.Invoke(null, new object[] { collection });
			return list;
		}

		private static List<object> ConvertCollectionToObjectList<T>(IList<T> original) {
			return original.Select(x => (object) x).ToList();
		}


		private static RawExpressionMetadata ConvertConstantExpression(Expression constantExpression, ExpressionModelMetadata modelMetadata) {
			if (!ExpressionConverterUtilities.TryDynamicInvoke(constantExpression, out var value)) {
				throw new NotSupportedException();
			}

			return CreateConstantRawExpressionMetadata(constantExpression.Type, value);
		}

		private static RawExpressionMetadata ConvertMemberExpression(MemberExpression memberExpression, ExpressionModelMetadata modelMetadata) {
		if (ExpressionConverterUtilities.TryGetDatePartColumnMemberPath(memberExpression, modelMetadata, out var datePartSourcePath, out var datePart)) {
			return CreateColumnRawExpressionMetadata(memberExpression, datePartSourcePath, metadata => {
				metadata.DatePart = datePart;
			});
		}

		if (ExpressionConverterUtilities.TryGetColumnMemberPath(memberExpression, modelMetadata, out var purePath)) {
			return CreateColumnRawExpressionMetadata(memberExpression, purePath);
		}

		if (ExpressionConverterUtilities.TryDynamicInvoke(memberExpression, out var value)) {
			return CreateConstantRawExpressionMetadata(memberExpression.Type, value);
		}

		throw new ExpressionConvertException();
	}

		private static RawExpressionMetadata CreateConstantRawExpressionMetadata(Type type, object value) {
			return new RawExpressionMetadata() {
				Type = ExpressionType.Constant,
				Parameter = new ExpressionMetadataParameter() {
					Type = type,
					Value = value
				}
			};
		}

		private static RawExpressionMetadata CreateColumnRawExpressionMetadata(Expression expression, string path, Action<RawExpressionMetadata> action = null) {
			var metadata = new RawExpressionMetadata() {
				Type = expression.NodeType,
				Parameter = new ExpressionMetadataParameter() {
					Type = expression.Type,
					ColumnPath = path
				}
			};
			action?.Invoke(metadata);
			return metadata;
		}

		private static RawExpressionMetadata ConvertBinaryExpression(BinaryExpression binaryExpression, ExpressionModelMetadata modelMetadata) {
			var left = Parse(binaryExpression.Left, modelMetadata);
			var right = Parse(binaryExpression.Right, modelMetadata);
			return new RawExpressionMetadata() {
				Type = binaryExpression.NodeType,
				Left = left,
				Right = right
			};
		}

		private static RawExpressionMetadata ConvertUnaryExpression(UnaryExpression unaryExpression, ExpressionModelMetadata modelMetadata) {
			if (unaryExpression.NodeType == ExpressionType.Convert) {
				return ConvertExpressionToValue(unaryExpression, modelMetadata);
			}

			if (unaryExpression.NodeType == ExpressionType.Not) {
				return ParseNotExpression(unaryExpression, modelMetadata);
			}
			if (unaryExpression.Operand is LambdaExpression lambdaExpression) {
				return Parse(lambdaExpression.Body, modelMetadata);
			}

			if (unaryExpression.Operand is UnaryExpression operandAsUnaryExpression) {
				return Parse(operandAsUnaryExpression, modelMetadata);
			}

			if (unaryExpression.Operand is MemberExpression memberExpression) {
				return Parse(memberExpression, modelMetadata);
			}


			throw new ExpressionConvertException();
		}

		private static RawExpressionMetadata ParseNotExpression(UnaryExpression unaryExpression, ExpressionModelMetadata modelMetadata) {
			var innerRawExpressionMetadata = Parse(unaryExpression.Operand, modelMetadata);
			if (innerRawExpressionMetadata.Type == ExpressionType.Constant) {
				return ParseConstantNotExpression(innerRawExpressionMetadata, modelMetadata);
			}

			if (innerRawExpressionMetadata.Type == ExpressionType.MemberAccess) {
				return ParseMemberAccessNotExpression(innerRawExpressionMetadata, modelMetadata);
			}

			if (ExpressionConverterUtilities.IsComparisonType(innerRawExpressionMetadata.Type)) {
				return ParseConditionNotExpression(innerRawExpressionMetadata, modelMetadata);
			}
			throw new ExpressionConvertException();
		}

		private static RawExpressionMetadata ParseMemberAccessNotExpression(RawExpressionMetadata innerRawExpressionMetadata, ExpressionModelMetadata modelMetadata) {
			innerRawExpressionMetadata.IsNot = true;
			return innerRawExpressionMetadata;
		}

		private static RawExpressionMetadata ParseConditionNotExpression(RawExpressionMetadata innerRawExpressionMetadata, ExpressionModelMetadata modelMetadata) {
			innerRawExpressionMetadata.IsNot = true;
			return innerRawExpressionMetadata;
		}

		private static RawExpressionMetadata ParseConstantNotExpression(RawExpressionMetadata innerRawExpressionMetadata, ExpressionModelMetadata modelMetadata) {
			if (innerRawExpressionMetadata.Parameter.Type == typeof(bool)) {
				innerRawExpressionMetadata.Parameter.Value = ! (bool)innerRawExpressionMetadata.Parameter.Value;
				return innerRawExpressionMetadata;
			}
			throw new ExpressionConvertException();
		}

		private static RawExpressionMetadata ConvertExpressionToValue(UnaryExpression unaryExpression, ExpressionModelMetadata modelMetadata) {
			if (ExpressionConverterUtilities.TryDynamicInvoke(unaryExpression, out var value)) {
				return CreateConstantRawExpressionMetadata(unaryExpression.Type, value);
			}

			return Parse(unaryExpression.Operand, modelMetadata);
		}

		
	}
}
