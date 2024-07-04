namespace ATF.Repository.Mock
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Linq.Expressions;
	using ATF.Repository.Mock.Internal;
	using ATF.Repository.Providers;
	using Terrasoft.Common;
	using Terrasoft.Nui.ServiceModel.DataContract;

	#region Class: InMemoryDataProviderMock

	public class InMemoryDataProviderMock : IDataProvider
	{
		#region Fields: Private

		private readonly DataSet _dataSet = new DataSet();

		#endregion

		#region Properties: Public

		private IDataStore _dataStore;
		public IDataStore DataStore => _dataStore ?? (_dataStore = new DataStore(_dataSet));

		#endregion

		#region Methods: Private

		private List<Dictionary<string, object>> ConvertFilteredItemsToRequestedDictionaries(ISelectQuery selectQuery,
			List<DataRow> filteredItems) {
			var response = new List<Dictionary<string, object>>();
			var table = _dataSet.Tables[selectQuery.RootSchemaName];
			var expressionContext = new ExpressionContext(table);
			var columnValueExtractors = new Dictionary<string, Tuple<object, Type>>();
			selectQuery.Columns.Items.ForEach(item => {
				var columnValueExpressionResponse = GetColumnValueExpression(expressionContext, item.Value);
				var extractorMethod = GenerateExtractorMethod(expressionContext, columnValueExpressionResponse.Item1,
					columnValueExpressionResponse.Item2);
				columnValueExtractors.Add(item.Key, new Tuple<object, Type>(extractorMethod, columnValueExpressionResponse.Item2));
			});

			filteredItems.ForEach(filteredItem => {
				var columnValues = new Dictionary<string, object>();
				columnValueExtractors.ForEach(columnValueExtractorItem => {
					var extractorItem = columnValueExtractorItem.Value;
					var value = ExtractRowValue(filteredItem, extractorItem.Item1, extractorItem.Item2);
					columnValues.Add(columnValueExtractorItem.Key, value);
				});
				response.Add(columnValues);
			});
			return response;
		}

		private object ExtractRowValue(DataRow filteredItem, object extractorFunc, Type resultType) {
			var method =
				RepositoryReflectionUtilities.GetGenericMethod(this.GetType(), "ExtractRowValue", resultType);
			return method.Invoke(this, new object[] { filteredItem, extractorFunc });
		}

		private T ExtractRowValue<T>(DataRow dataRow, object extractorFunc) {
			if (extractorFunc is Func<DataRow, T> typedExtractorFunc) {
				return typedExtractorFunc.Invoke(dataRow);
			}

			return default(T);
		}

		private object GenerateExtractorMethod(ExpressionContext expressionContext, Expression valueExpression, Type type) {
			var method =
				RepositoryReflectionUtilities.GetGenericMethod(this.GetType(), "GenerateExtractorMethod", type);
			return method.Invoke(this, new object[] { expressionContext, valueExpression });
		}

		private Func<DataRow, T> GenerateExtractorMethod<T>(ExpressionContext expressionContext, Expression valueExpression) {
			var lambda = Expression.Lambda(valueExpression, expressionContext.RowExpression);
			var iterationExpression = (Expression<Func<DataRow, T>>)lambda;
			var iterationExtractor = iterationExpression.Compile();
			return iterationExtractor;
		}

		private Func<DataRow, string, T> GetExtractorMethod<T>(Expression extractorExpression) {
			var typedExtractorExpression = (Expression<Func<DataRow, string, T>>)extractorExpression;
			throw new NotImplementedException();

		}

		private Tuple<Expression, Type>  GetColumnValueExpression(ExpressionContext expressionContext, ISelectQueryColumn itemValue) {
			return ExpressionBuilder.BuildColumnValueExtractor(expressionContext, itemValue);
		}

		private List<DataRow> GetFilteredItems(ISelectQuery selectQuery, bool isAggregateQuery) {
			var table = _dataSet.Tables[selectQuery.RootSchemaName];
			var expressionContext = new ExpressionContext(table);
			var filterExpression = ExpressionBuilder.BuildFilter(expressionContext, selectQuery);
			var sortExpression = ExpressionBuilder.BuildSortExpression(expressionContext, selectQuery);
			var filteredItems = table.GetFilteredItems(filterExpression);
			return isAggregateQuery
				? filteredItems
				: filteredItems.GetSortedItems(expressionContext, sortExpression)
					.SkipItems(selectQuery.RowsOffset).TakeItems(selectQuery.RowCount);

		}

		#endregion

		#region Methods: Public

		public IDefaultValuesResponse GetDefaultValues(string schemaName) {
			throw new System.NotImplementedException();
		}

		public IItemsResponse GetItems(ISelectQuery selectQuery) {
			var isAggregateQuery = IsSingleColumnAggregateQuery(selectQuery);
			var filteredItems = GetFilteredItems(selectQuery, isAggregateQuery);
			var items = isAggregateQuery
				? ConvertFilteredItemsToAggregateDictionaries(selectQuery, filteredItems)
				: ConvertFilteredItemsToRequestedDictionaries(selectQuery, filteredItems);
			return new ATF.Repository.Mock.Internal.ItemsResponse() {
				Success = true,
				ErrorMessage = null,
				Items = items
			};
		}

		private List<Dictionary<string, object>> ConvertFilteredItemsToAggregateDictionaries(ISelectQuery selectQuery, List<DataRow> filteredItems) {
			var columnPair = selectQuery.Columns.Items.First();
			var column = columnPair.Value;
			var table = _dataSet.Tables[selectQuery.RootSchemaName];
			var expressionContext = new ExpressionContext(table);
			var columnPath = column.Expression.FunctionArgument.ColumnPath;
			var columnValuePath = table.GetSchemaPathDataType(columnPath);
			var aggregateExpression = ExpressionBuilder.GetSingleAggregationExpression(expressionContext,
				column.Expression.AggregationType, columnValuePath, expressionContext.RowsExpression,
				columnPath);
			var resultType = column.Expression.AggregationType == AggregationType.Count
				? typeof(int)
				: columnValuePath;
			var genericInvokeAggregateCollectionExpressionMethod =
				RepositoryReflectionUtilities.GetGenericMethod(GetType(), "InvokeAggregateCollectionExpression",
					resultType);
			var aggregateValue = genericInvokeAggregateCollectionExpressionMethod.Invoke(this,
				new object[] { expressionContext, aggregateExpression, filteredItems });
			return new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{columnPair.Key, aggregateValue}
				}
			};
		}

		private T InvokeAggregateCollectionExpression<T>(ExpressionContext expressionContext, Expression aggregateExpression, List<DataRow> items) {
			var aggregateLambdaExpression =
				(Expression<Func<List<DataRow>, T>>)Expression.Lambda(aggregateExpression,
					expressionContext.RowsExpression);
			var aggregateMethod = aggregateLambdaExpression.Compile();
			var response = aggregateMethod.Invoke(items);
			return response;
		}

		private bool IsSingleColumnAggregateQuery(ISelectQuery selectQuery) {
			var column = selectQuery.Columns.Items.First().Value;
			return selectQuery.Columns.Items.Count == 1 && column.Expression.FunctionType != FunctionType.None;
		}

		public IExecuteResponse BatchExecute(List<IBaseQuery> queries) {
			throw new System.NotImplementedException();
		}

		public T GetSysSettingValue<T>(string sysSettingCode) {
			throw new System.NotImplementedException();
		}

		public bool GetFeatureEnabled(string featureCode) {
			throw new System.NotImplementedException();
		}

		#endregion

	}

	#endregion

}
