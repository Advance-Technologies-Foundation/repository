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

		private static readonly MethodInfo OrderByMethodInfo =  typeof(Enumerable)
			.GetMethods().First(x =>
				x.Name == "OrderBy" && x.GetParameters().Length == 2);

		private static readonly MethodInfo ThenByMethodInfo =  typeof(Enumerable)
			.GetMethods().First(x =>
				x.Name == "ThenBy" && x.GetParameters().Length == 2);

		private static readonly MethodInfo OrderByDescendingMethodInfo =  typeof(Enumerable)
			.GetMethods().First(x =>
				x.Name == "OrderByDescending" && x.GetParameters().Length == 2);

		private static readonly MethodInfo ThenByDescendingMethodInfo =  typeof(Enumerable)
			.GetMethods().First(x =>
				x.Name == "ThenByDescending" && x.GetParameters().Length == 2);

		private static readonly MethodInfo StartsWithMethodInfo = typeof(string)
			.GetMethods().First(x =>
				x.Name == "StartsWith" && x.GetParameters().Length == 1 &&
				x.GetParameters()[0].ParameterType == typeof(string));

		private static readonly MethodInfo ContainsMethodInfo = typeof(string)
			.GetMethods().First(x =>
				x.Name == "Contains" && x.GetParameters().Length == 1 &&
				x.GetParameters()[0].ParameterType == typeof(string));

		private static readonly MethodInfo EndsWithMethodInfo = typeof(string)
			.GetMethods().First(x =>
				x.Name == "EndsWith" && x.GetParameters().Length == 1 &&
				x.GetParameters()[0].ParameterType == typeof(string));

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

			if (filter.FilterType == FilterType.InFilter) {
				return BuildInFilter(expressionContext, filter);
			}
			throw new NotImplementedException();
		}

		private static Expression BuildInFilter(ExpressionContext expressionContext, IFilter filter) {
			var leftExpression = BuildCompareFilterPart(expressionContext, filter.LeftExpression);
			Expression expression = null;
			filter.RightExpressions.ForEach(item => {
				var rightExpression = BuildCompareFilterPart(expressionContext, item);
				var comparisonExpression = BuildCompareFilter(filter.ComparisonType, leftExpression, rightExpression);
				expression = expression != null
					? Expression.Or(expression, comparisonExpression)
					: comparisonExpression;
			});
			var blockExpression = Expression.Block(expression);
			var hasValueExpression = BuildHasValueFilterPart(expressionContext, filter.LeftExpression.ExpressionType, filter.LeftExpression.ColumnPath);
			if (hasValueExpression != null) {
				return Expression.And(hasValueExpression, blockExpression);
			}

			return blockExpression;
		}

		private static Expression GetDetailExpression(Expression rowParameterExpression, string columnPath) {
			var columnNameExpression = Expression.Constant(columnPath, typeof(string));
			return Expression.Call(null, DetailMethodInfo, rowParameterExpression, columnNameExpression);
		}

		private static Expression BuildExistsFilter(ExpressionContext expressionContext, IFilter filter) {
			var schemaPath = expressionContext.ContextTable.GetSchemaPath(filter.LeftExpression.ColumnPath);
			var hasValueExpression = BuildHasValueFilterPart(expressionContext, filter.LeftExpression.ExpressionType,
				schemaPath.FullDetailPath);
			var nestedContext = expressionContext.GetNestedExpressionContext(schemaPath.DetailPart.DetailDataTable);
			var detailExpression =
				GetDetailExpression(expressionContext.RowExpression, schemaPath.FullDetailPath);
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
			var hasValueExpression = BuildHasValueFilterPart(expressionContext, filter.LeftExpression.ExpressionType, filter.LeftExpression.ColumnPath);
			if (hasValueExpression != null) {
				return Expression.And(hasValueExpression, comparisonExpression);
			}

			return comparisonExpression;
		}

		private static Expression BuildHasValueFilterPart(ExpressionContext expressionContext, EntitySchemaQueryExpressionType expressionType, string columnPath) {
			if (expressionType != EntitySchemaQueryExpressionType.SchemaColumn &&
				expressionType != EntitySchemaQueryExpressionType.SubQuery) {
				return null;
			}

			var columnNameExpression = Expression.Constant(columnPath, typeof(string));
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
			if (comparisonType == FilterComparisonType.StartWith) {
				return GetStartsWithExpression(leftExpression, rightExpression);
			}
			if (comparisonType == FilterComparisonType.NotStartWith) {
				return Expression.Not(GetStartsWithExpression(leftExpression, rightExpression));
			}
			if (comparisonType == FilterComparisonType.EndWith) {
				return GetEndsWithExpression(leftExpression, rightExpression);
			}
			if (comparisonType == FilterComparisonType.NotEndWith) {
				return Expression.Not(GetEndsWithExpression(leftExpression, rightExpression));
			}
			if (comparisonType == FilterComparisonType.Contain) {
				return GetContainsExpression(leftExpression, rightExpression);
			}
			if (comparisonType == FilterComparisonType.NotContain) {
				return Expression.Not(GetContainsExpression(leftExpression, rightExpression));
			}
			throw new NotImplementedException();
		}

		private static Expression GetStartsWithExpression(Expression leftExpression, Expression rightExpression) {
			return Expression.Call(leftExpression, StartsWithMethodInfo, rightExpression);
		}

		private static Expression GetEndsWithExpression(Expression leftExpression, Expression rightExpression) {
			return Expression.Call(leftExpression, EndsWithMethodInfo, rightExpression);
		}

		private static Expression GetContainsExpression(Expression leftExpression, Expression rightExpression) {
			return Expression.Call(leftExpression, ContainsMethodInfo, rightExpression);
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
				GetDetailExpression(expressionContext.RowExpression, schemaPath.FullDetailPath);
			var filterExpression = BuildFilter(nestedContext, filterLeftExpression.SubFilters);
			if (filterExpression != null) {
				detailExpression = Expression.Call(typeof(Enumerable), "Where", new[] { typeof(DataRow) },
					detailExpression, Expression.Lambda(filterExpression, nestedContext.RowExpression));
			}
			var columnValueType = schemaPath.Last.PathItems.Last().DataColumn.DataType;
			return GetSingleAggregationExpression(nestedContext, filterLeftExpression.AggregationType, columnValueType,
				detailExpression, schemaPath.Last.Path);
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
			var actualValue = ValueBuilder.GetActualValue(filterLeftExpression.Parameter);
			return valueType != actualValueType
				? (Expression)Expression.Convert(Expression.Constant(actualValue), valueType)
				: Expression.Constant(actualValue, valueType ?? actualValueType);
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

		internal static Expression BuildQueryFilter(ExpressionContext expressionContext, IFilterGroup filterGroup) {
			var filterExpression = BuildFilter(expressionContext, filterGroup) ?? BuildEmptyFilterExpression(expressionContext.RowExpression);
			var lambda = Expression.Lambda(filterExpression, expressionContext.RowExpression);
			return lambda;
		}

		internal static Expression BuildSortExpression(ExpressionContext expressionContext, ISelectQuery selectQuery) {
			if (!selectQuery.Columns.Items.Any(x => x.Value.OrderPosition > 0)) {
				return null;
			}

			Expression sortExpression = expressionContext.RowsExpression;
			var columnsToSort = selectQuery.Columns.Items.Where(x => x.Value.OrderPosition >= 0)
				.OrderBy(x => x.Value.OrderPosition).ToList();
			var first = columnsToSort.First();
			columnsToSort.ForEach(item => {
				sortExpression = BuildSortExpression(expressionContext, sortExpression, item.Value, item.Value == first.Value);
				});
			return sortExpression;
		}

		private static Expression BuildSortExpression(ExpressionContext expressionContext, Expression sourceExpression, ISelectQueryColumn column, bool first) {
			var methodInfoTemplate = GetSortMethodInfoTemplate(column.OrderDirection, first);
			if (methodInfoTemplate == null) {
				return sourceExpression;
			}
			var columnValueType = expressionContext.ContextTable.GetSchemaPathDataType(column.Expression.ColumnPath);
			var nestedContext = expressionContext.GetNestedExpressionContext(expressionContext.ContextTable);
			var subValueExpression = GetDataRowFieldExpression(nestedContext.RowExpression, column.Expression.ColumnPath,
				columnValueType);
			var sortLambdaExpression = Expression.Lambda(subValueExpression, nestedContext.RowExpression);
			var methodInfo = methodInfoTemplate.MakeGenericMethod(typeof(DataRow), columnValueType);
			return Expression.Call(null, methodInfo, sourceExpression, sortLambdaExpression);
		}

		private static MethodInfo GetSortMethodInfoTemplate(OrderDirection columnOrderDirection, bool first) {
			switch (columnOrderDirection) {
				case OrderDirection.Ascending:
					return first
						? OrderByMethodInfo
						: ThenByMethodInfo;
				case OrderDirection.Descending:
					return first
						? OrderByDescendingMethodInfo
						: ThenByDescendingMethodInfo;
				case OrderDirection.None:
				default:
					return null;
			}
		}

		internal static Tuple<Expression, Type> BuildColumnValueExtractor(ExpressionContext expressionContext,
			ISelectQueryColumn selectQueryColumn) {
			return BuildCompareSchemaColumnFilterPart(expressionContext, selectQueryColumn.Expression.ColumnPath);
		}

		internal static Expression GetSingleAggregationExpression(ExpressionContext expressionContext, AggregationType aggregationType, Type columnValueType, Expression source, string path) {
			var methodInfo = GetAggregationMethodInfo(aggregationType, columnValueType);
			if (aggregationType == AggregationType.Count) {
				return Expression.Call(null, methodInfo, source);
			}
			var subValueExpression = GetDataRowFieldExpression(expressionContext.RowExpression, path,
				columnValueType);
			var aggregationExpression = Expression.Lambda(subValueExpression, expressionContext.RowExpression);
			return Expression.Call(null, methodInfo, source, aggregationExpression);
		}

		#endregion

	}

	#endregion

}
