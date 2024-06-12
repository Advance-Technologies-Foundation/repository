namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;
	using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;

	#region Class: ExpressionBuilder

	internal class ExpressionBuilder
	{

		#region Fields: Private

		private static readonly MethodInfo FieldMethodInfo = typeof(ExpressionBuilderExtensions).GetMethods().First(x =>
			x.Name == "GetTypedPathValue" && x.IsGenericMethod && x.GetParameters().Length == 2 &&
			x.GetParameters()[1].ParameterType == typeof(string));
		private static readonly MethodInfo PathHasValue = typeof(ExpressionBuilderExtensions).GetMethods().First(x =>
			x.Name == "PathHasValue" && x.GetParameters().Length == 2 &&
			x.GetParameters()[1].ParameterType == typeof(string));

		#endregion

		#region Methods: Private

		private static Expression GetDataRowFieldExpression(Expression rowParameterExpression, string columnName,
			Type valueType) {
			var columnNameExpression = Expression.Constant(columnName, typeof(string));
			var genericMethod = FieldMethodInfo.MakeGenericMethod(valueType);
			return Expression.Call(null, genericMethod, rowParameterExpression, columnNameExpression);
		}

		private static LambdaExpression BuildEmptyFilterExpression(ParameterExpression parameterExpression) {
			throw new NotImplementedException();
		}

		private static Expression BuildFilter(ExpressionContext expressionContext, IFilter filter) {
			if (!filter.IsEnabled) {
				return null;
			}
			if (filter.FilterType == FilterType.FilterGroup) {
				return BuildFilterGroup(expressionContext, filter);
			}

			if (filter.FilterType == FilterType.CompareFilter) {
				return BuildCompareFilter(expressionContext, filter);
			}

			throw new NotImplementedException();
		}

		private static Expression BuildCompareFilter(ExpressionContext expressionContext, IFilter filter) {
			var leftExpression = BuildCompareFilterPart(expressionContext, filter.LeftExpression);
			var rightExpression = BuildCompareFilterPart(expressionContext, filter.RightExpression);
			var comparisonExpression = BuildCompareFilter(filter.ComparisonType, leftExpression, rightExpression);
			var hasValueExpression = BuildHasValueFilterPart(expressionContext, filter.LeftExpression);
			if (hasValueExpression != null) {
				return Expression.And(hasValueExpression, comparisonExpression);
			}

			return comparisonExpression;
		}

		private static Expression BuildHasValueFilterPart(ExpressionContext expressionContext, IBaseExpression filterExpression) {
			if (filterExpression.ExpressionType != EntitySchemaQueryExpressionType.SchemaColumn) {
				return null;
			}
			var columnNameExpression = Expression.Constant(filterExpression.ColumnPath, typeof(string));
			return Expression.Call(null, PathHasValue, expressionContext.RowExpression, columnNameExpression);
		}

		private static Expression BuildCompareFilter(FilterComparisonType comparisonType, Expression leftExpression, Expression rightExpression) {
			if (comparisonType == FilterComparisonType.Equal) {
				return Expression.Equal(leftExpression, rightExpression);
			}
			if (comparisonType == FilterComparisonType.NotEqual) {
				return Expression.NotEqual(leftExpression, rightExpression);
			}
			if (comparisonType == FilterComparisonType.Greater) {
				return Expression.GreaterThan(leftExpression, rightExpression);
			}
			if (comparisonType == FilterComparisonType.GreaterOrEqual) {
				return Expression.GreaterThanOrEqual(leftExpression, rightExpression);
			}
			if (comparisonType == FilterComparisonType.Less) {
				return Expression.LessThan(leftExpression, rightExpression);
			}
			if (comparisonType == FilterComparisonType.LessOrEqual) {
				return Expression.LessThanOrEqual(leftExpression, rightExpression);
			}
			throw new NotImplementedException();
		}

		private static Expression BuildCompareFilterPart(ExpressionContext expressionContext, IBaseExpression filterLeftExpression) {
			if (filterLeftExpression.ExpressionType == EntitySchemaQueryExpressionType.SchemaColumn) {
				var columnFilterPart = BuildCompareSchemaColumnFilterPart(expressionContext, filterLeftExpression);
				return columnFilterPart.Item1;
			}
			if (filterLeftExpression.ExpressionType == EntitySchemaQueryExpressionType.Parameter) {
				return BuildCompareParameterFilterPart(expressionContext, filterLeftExpression);
			}

			throw new NotImplementedException();
		}

		private static Expression BuildCompareParameterFilterPart(ExpressionContext expressionContext, IBaseExpression filterLeftExpression) {
			var valueType = filterLeftExpression.Parameter.DataValueType.GetValueType();
			var actualValueType = filterLeftExpression.Parameter.Value?.GetType() ?? null;
			return valueType != actualValueType
				? (Expression)Expression.Convert(Expression.Constant(filterLeftExpression.Parameter.Value), valueType)
				: Expression.Constant(filterLeftExpression.Parameter.Value, valueType);
		}

		private static Tuple<Expression, Type> BuildCompareSchemaColumnFilterPart(ExpressionContext expressionContext, IBaseExpression schemaColumnFilter) {
			return BuildCompareSchemaColumnFilterPart(expressionContext, schemaColumnFilter.ColumnPath);
		}

		private static Tuple<Expression, Type> BuildCompareSchemaColumnFilterPart(ExpressionContext expressionContext, string columnPath) {
			var columnValueType = expressionContext.ContextTable.GetColumnPathDataType(columnPath);
			return new Tuple<Expression, Type>(
				GetDataRowFieldExpression(expressionContext.RowExpression, columnPath,
					columnValueType), columnValueType);
		}

		private static Expression BuildFilterGroup(ExpressionContext expressionContext, IFilter filterGroup) {
			var filtersList = new List<Expression>();
			foreach (var filterItem in filterGroup.Items) {
				var filterExpression = BuildFilter(expressionContext, filterItem.Value);
				if (filterExpression != null) {
					filtersList.Add(filterExpression);
				}
			}

			if (filtersList.Count < 2) {
				return filtersList.FirstOrDefault();
			}

			var responseExpression = filtersList.First();
			foreach (var filterExpression in filtersList.Skip(1)) {
				responseExpression = filterGroup.LogicalOperation == LogicalOperationStrict.And
					? Expression.And(responseExpression, filterExpression)
					: Expression.Or(responseExpression, filterExpression);
			}

			return responseExpression;
		}

		#endregion

		#region Methods: Internal

		internal static Expression BuildFilter(ExpressionContext expressionContext, ISelectQuery selectQuery) {
			var filterExpression = BuildFilter(expressionContext, selectQuery.Filters) ?? BuildEmptyFilterExpression(expressionContext.RowExpression);
			var lambda = Expression.Lambda(filterExpression, expressionContext.RowExpression);
			return lambda;
		}

		internal static Tuple<Expression, Type> BuildColumnValueExtractor(ExpressionContext expressionContext,
			ISelectQueryColumn selectQueryColumn) {
			return BuildCompareSchemaColumnFilterPart(expressionContext, selectQueryColumn.Expression.ColumnPath);
		}

		#endregion

	}

	#endregion

}
