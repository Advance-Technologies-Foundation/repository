namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;
	using Terrasoft.Nui.ServiceModel.DataContract;
	using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;

	#region Class: ExpressionBuilder

	internal class ExpressionBuilder
	{

		#region Fields: Private

		private static readonly MethodInfo GetTypedPathValueMethodInfo = typeof(ExpressionBuilderExtensions)
			.GetMethods().First(x =>
				x.Name == "GetTypedPathValue" && x.IsGenericMethod && x.GetParameters().Length == 2 &&
				x.GetParameters()[1].ParameterType == typeof(string));

		private static readonly MethodInfo HasTypedPathValueMethodInfo = typeof(ExpressionBuilderExtensions)
			.GetMethods().First(x =>
				x.Name == "HasTypedPathValue" && x.GetParameters().Length == 2 &&
				x.GetParameters()[1].ParameterType == typeof(string));

		private static readonly MethodInfo DetailMethodInfo = typeof(ExpressionBuilderExtensions)
			.GetMethods().First(x =>
				x.Name == "Detail" && x.GetParameters().Length == 2 &&
				x.GetParameters()[1].ParameterType == typeof(string));

		#endregion

		#region Methods: Private

		private static Expression GetDataRowFieldExpression(Expression rowParameterExpression, string columnName,
			Type valueType) {
			var columnNameExpression = Expression.Constant(columnName, typeof(string));
			var genericMethod = GetTypedPathValueMethodInfo.MakeGenericMethod(valueType);
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

			if (filter.FilterType == FilterType.Exists) {
				return BuildExistsFilter(expressionContext, filter);
			}
			throw new NotImplementedException();
		}

		private static Expression GetDetailExpression(Expression rowParameterExpression, string columnPath) {
			var columnNameExpression = Expression.Constant(columnPath, typeof(string));
			return Expression.Call(null, DetailMethodInfo, rowParameterExpression, columnNameExpression);
		}

		private static Expression BuildExistsFilter(ExpressionContext expressionContext, IFilter filter) {
			var columnPath = filter.LeftExpression.ColumnPath;
			var schemaPath = expressionContext.ContextTable.GetSchemaPath(columnPath);
			var hasValueExpression = BuildHasValueFilterPart(expressionContext, filter.LeftExpression);
			var nestedContext = expressionContext.GetNestedExpressionContext(schemaPath.DetailPart.DetailDataTable);
			var detailExpression =
				GetDetailExpression(expressionContext.RowExpression, filter.LeftExpression.ColumnPath);
			var filterExpression = BuildFilter(nestedContext, filter.SubFilters);
			var anyExpression = filterExpression != null
				? Expression.Call(typeof(Enumerable), "Any", new[] { typeof(DataRow) },
				detailExpression, Expression.Lambda(filterExpression, nestedContext.RowExpression))
				: Expression.Call(typeof(Enumerable), "Any", new[] { typeof(DataRow) },
					detailExpression);
			return Expression.And(hasValueExpression, anyExpression);
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
			if (filterExpression.ExpressionType != EntitySchemaQueryExpressionType.SchemaColumn &&
				filterExpression.ExpressionType != EntitySchemaQueryExpressionType.SubQuery) {
				return null;
			}

			var columnNameExpression = Expression.Constant(filterExpression.ColumnPath, typeof(string));
			return Expression.Call(null, HasTypedPathValueMethodInfo, expressionContext.RowExpression, columnNameExpression);
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
			if (filterLeftExpression.ExpressionType == EntitySchemaQueryExpressionType.SubQuery) {
				return BuildCompareSubFilterPart(expressionContext, filterLeftExpression);
			}

			throw new NotImplementedException();
		}

		private static Expression BuildCompareSubFilterPart(ExpressionContext expressionContext, IBaseExpression filterLeftExpression) {
			var columnPath = filterLeftExpression.ColumnPath;

			var schemaPath = expressionContext.ContextTable.GetSchemaPath(columnPath);
			var nestedContext = expressionContext.GetNestedExpressionContext(schemaPath.DetailPart.DetailDataTable);
			var detailExpression =
				GetDetailExpression(expressionContext.RowExpression, filterLeftExpression.ColumnPath);
			var filterExpression = BuildFilter(nestedContext, filterLeftExpression.SubFilters);
			if (filterExpression != null) {
				detailExpression = Expression.Call(typeof(Enumerable), "Where", new[] { typeof(DataRow) },
					detailExpression, Expression.Lambda(filterExpression, nestedContext.RowExpression));
			}
			var columnValueType = schemaPath.Last.PathItems.Last().DataColumn.DataType;
			var methodInfo = GetAggregationMethodInfo(filterLeftExpression.AggregationType, columnValueType);
			if (filterLeftExpression.AggregationType == AggregationType.Count) {
				return Expression.Call(null, methodInfo, detailExpression);
			}
			var subValueExpression = GetDataRowFieldExpression(nestedContext.RowExpression, schemaPath.Last.Path,
				columnValueType);
			var aggregationExpression = Expression.Lambda(subValueExpression, nestedContext.RowExpression);
			return Expression.Call(null, methodInfo, detailExpression, aggregationExpression);
		}

		private static MethodInfo GetAggregationMethodInfo(AggregationType aggregationType, Type columnValueType) {
			if (aggregationType == AggregationType.Sum) {
				return GetGenericTypedAggregationMethodInfo("Sum", columnValueType);
			}

			if (aggregationType == AggregationType.Min) {
				return GetGenericTypedAggregationMethodInfo("Min", columnValueType);
			}

			if (aggregationType == AggregationType.Max) {
				return GetGenericTypedAggregationMethodInfo("Max", columnValueType);
			}

			if (aggregationType == AggregationType.Avg) {
				return GetGenericTypedAggregationMethodInfo("Average", columnValueType);
			}

			if (aggregationType == AggregationType.Count) {
				return GetGenericTypedAggregationMethodInfo("Count", columnValueType);
			}

			throw new NotImplementedException();
		}

		private static MethodInfo GetGenericTypedAggregationMethodInfo(string name, Type columnValueType) {
			var methods = typeof(Enumerable).GetMethods().Where(x=>x.Name == name).ToList();
			var methodInfo = methods.FirstOrDefault(x =>
					x.IsGenericMethod && x.ReturnType == columnValueType);
			if (methodInfo != null) {
				return methodInfo.MakeGenericMethod(typeof(DataRow));
			}

			methodInfo = methods.FirstOrDefault(x =>
					x.IsGenericMethod && x.GetGenericArguments().Length == 2);
			if (methodInfo != null) {
				return methodInfo.MakeGenericMethod(typeof(DataRow), columnValueType);
			}

			methodInfo = methods.FirstOrDefault(x =>
				x.IsGenericMethod && x.GetGenericArguments().Length == 1 && x.ReturnType == typeof(int));
			if (methodInfo != null) {
				return methodInfo.MakeGenericMethod(typeof(DataRow));
			}
			throw new NotImplementedException(
				$"Not found aggregation function {name} for result type {columnValueType.Name}");
		}

		private static Expression BuildCompareParameterFilterPart(ExpressionContext expressionContext, IBaseExpression filterLeftExpression) {
			var valueType = filterLeftExpression.Parameter.DataValueType.GetValueType();
			var actualValueType = filterLeftExpression.Parameter.Value?.GetType() ?? null;
			var actualValue = GetActualValue(filterLeftExpression.Parameter);
			return valueType != actualValueType
				? (Expression)Expression.Convert(Expression.Constant(actualValue), valueType)
				: Expression.Constant(actualValue, valueType ?? actualValueType);
		}

		private static object GetActualValue(IParameter parameter) {
			var p = new Parameter() {
				Value = parameter.Value,
				DataValueType = parameter.DataValueType
			};
			var value = p.GetValue(null);
			if (parameter.DataValueType.GetValueType() == typeof(DateTime) && value is DateTime dateTimeValue) {
				return new DateTime(dateTimeValue.Year, dateTimeValue.Month, dateTimeValue.Day, dateTimeValue.Hour,
					dateTimeValue.Minute, dateTimeValue.Second);
			}
			return value;
		}

		private static Tuple<Expression, Type> BuildCompareSchemaColumnFilterPart(ExpressionContext expressionContext, IBaseExpression schemaColumnFilter) {
			return BuildCompareSchemaColumnFilterPart(expressionContext, schemaColumnFilter.ColumnPath);
		}

		private static Tuple<Expression, Type> BuildCompareSchemaColumnFilterPart(ExpressionContext expressionContext, string columnPath) {
			var columnValueType = expressionContext.ContextTable.GetSchemaPathDataType(columnPath);
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
