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

	#region Class: MemoryDataProviderMock

	public class MemoryDataProviderMock : BaseDataProviderMock
	{
		#region Fields: Private

		private readonly DataSet _dataSet = new DataSet();

		#endregion

		#region Properties: Public

		private DataStore _dataStore;
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
			var filterExpression = ExpressionBuilder.BuildQueryFilter(expressionContext, selectQuery.Filters);
			var sortExpression = ExpressionBuilder.BuildSortExpression(expressionContext, selectQuery);
			var filteredItems = table.GetFilteredItems(filterExpression);
			return isAggregateQuery
				? filteredItems
				: filteredItems.GetSortedItems(expressionContext, sortExpression)
					.SkipItems(selectQuery.RowsOffset).TakeItems(selectQuery.RowCount);

		}

		private void ExecuteDeleteQuery(IDeleteQuery deleteQuery) {
			var table = _dataSet.Tables[deleteQuery.RootSchemaName];
			var expressionContext = new ExpressionContext(table);
			var filterExpression = ExpressionBuilder.BuildQueryFilter(expressionContext, deleteQuery.Filters);
			var filteredItems = table.GetFilteredItems(filterExpression);
			_dataStore.DeleteRecords(filteredItems);
		}

		private void ExecuteUpdateQuery(IUpdateQuery updateQuery) {
			var recordValues = GetSaveQueryValues(updateQuery.ColumnValues.Items);
			var table = _dataSet.Tables[updateQuery.RootSchemaName];
			var expressionContext = new ExpressionContext(table);
			var filterExpression = ExpressionBuilder.BuildQueryFilter(expressionContext, updateQuery.Filters);
			var filteredItems = table.GetFilteredItems(filterExpression);
			_dataStore.UpdateRecords(filteredItems, recordValues);
		}

		private void ExecuteInsertQuery(IInsertQuery insertQuery) {
			var recordValues = GetSaveQueryValues(insertQuery.ColumnValues.Items);
			_dataStore.InsertRecord(insertQuery.RootSchemaName, recordValues);
		}

		private Dictionary<string, object> GetSaveQueryValues(Dictionary<string, IColumnExpression> columnValuesItems) {
			var response = new Dictionary<string, object>();
			columnValuesItems.ForEach(x => {
				var actualValue = ValueBuilder.GetActualValue(x.Value.Parameter);
				response.Add(x.Key, actualValue);
			});
			return response;
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

			Type resultType;
			if (column.Expression.AggregationType == AggregationType.Count) {
				resultType = typeof(int);
			} else if (column.Expression.AggregationType == AggregationType.Avg &&
			           (columnValuePath == typeof(int) || columnValuePath == typeof(Int32))) {
				resultType = typeof(double);
			} else {
				resultType = columnValuePath;
			}

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

		private List<Dictionary<string, object>> ConvertFilteredItemsToGroupByDictionaries(ISelectQuery selectQuery, List<DataRow> filteredItems) {
			var table = _dataSet.Tables[selectQuery.RootSchemaName];
			var expressionContext = new ExpressionContext(table);

			var groupByColumns = new Dictionary<string, ISelectQueryColumn>();
			var aggregationColumns = new Dictionary<string, ISelectQueryColumn>();

			foreach (var columnPair in selectQuery.Columns.Items) {
				if (columnPair.Value.Expression.FunctionType == FunctionType.Aggregation) {
					aggregationColumns.Add(columnPair.Key, columnPair.Value);
				} else {
					groupByColumns.Add(columnPair.Key, columnPair.Value);
				}
			}

			var groupByExtractors = new Dictionary<string, Tuple<object, Type>>();
			foreach (var columnPair in groupByColumns) {
				var columnValueExpressionResponse = GetColumnValueExpression(expressionContext, columnPair.Value);
				var extractorMethod = GenerateExtractorMethod(expressionContext, columnValueExpressionResponse.Item1,
					columnValueExpressionResponse.Item2);
				groupByExtractors.Add(columnPair.Key, new Tuple<object, Type>(extractorMethod, columnValueExpressionResponse.Item2));
			}

			var groups = new Dictionary<string, List<DataRow>>();
			foreach (var item in filteredItems) {
				var keyValues = new List<string>();
				foreach (var extractor in groupByExtractors) {
					var value = ExtractRowValue(item, extractor.Value.Item1, extractor.Value.Item2);
					keyValues.Add(value?.ToString() ?? "NULL");
				}
				var groupKey = string.Join("|||", keyValues);

				if (!groups.ContainsKey(groupKey)) {
					groups[groupKey] = new List<DataRow>();
				}
				groups[groupKey].Add(item);
			}

			var result = new List<Dictionary<string, object>>();
			foreach (var group in groups) {
				var resultRow = new Dictionary<string, object>();

				var firstItemInGroup = group.Value.First();
				foreach (var extractor in groupByExtractors) {
					var value = ExtractRowValue(firstItemInGroup, extractor.Value.Item1, extractor.Value.Item2);
					resultRow.Add(extractor.Key, value);
				}

				foreach (var aggColumnPair in aggregationColumns) {
					var column = aggColumnPair.Value;
					var columnPath = column.Expression.FunctionArgument.ColumnPath;
					var columnValuePath = table.GetSchemaPathDataType(columnPath);

					var aggregateExpression = ExpressionBuilder.GetSingleAggregationExpression(expressionContext,
						column.Expression.AggregationType, columnValuePath, expressionContext.RowsExpression,
						columnPath);

					Type resultType;
					if (column.Expression.AggregationType == AggregationType.Count) {
						resultType = typeof(int);
					} else if (column.Expression.AggregationType == AggregationType.Avg &&
					           (columnValuePath == typeof(int) || columnValuePath == typeof(Int32))) {
						resultType = typeof(double);
					} else {
						resultType = columnValuePath;
					}

					var genericInvokeAggregateCollectionExpressionMethod =
						RepositoryReflectionUtilities.GetGenericMethod(GetType(), "InvokeAggregateCollectionExpression",
							resultType);

					var aggregateValue = genericInvokeAggregateCollectionExpressionMethod.Invoke(this,
						new object[] { expressionContext, aggregateExpression, group.Value });

					resultRow.Add(aggColumnPair.Key, aggregateValue);
				}

				result.Add(resultRow);
			}

			return result;
		}

		private bool IsSingleColumnAggregateQuery(ISelectQuery selectQuery) {
			var column = selectQuery.Columns.Items.First().Value;
			return selectQuery.Columns.Items.Count == 1 && column.Expression.FunctionType == FunctionType.Aggregation;
		}

		private bool IsGroupByQuery(ISelectQuery selectQuery) {
			return selectQuery.Columns.Items.Count > 1 &&
			       selectQuery.Columns.Items.Any(x => x.Value.Expression.FunctionType == FunctionType.Aggregation);
		}

		#endregion

		#region Methods: Public

		public override IDefaultValuesResponse GetDefaultValues(string schemaName) {
			return new Internal.DefaultValuesResponse() {
				Success = true,
				DefaultValues = _dataStore.GetDefaultValues(schemaName)
			};
		}

		public override IItemsResponse GetItems(ISelectQuery selectQuery) {
			var isGroupByQuery = IsGroupByQuery(selectQuery);
			var isAggregateQuery = IsSingleColumnAggregateQuery(selectQuery);
			var filteredItems = GetFilteredItems(selectQuery, isAggregateQuery || isGroupByQuery);

			List<Dictionary<string, object>> items;
			if (isGroupByQuery) {
				items = ConvertFilteredItemsToGroupByDictionaries(selectQuery, filteredItems);
			} else if (isAggregateQuery) {
				items = ConvertFilteredItemsToAggregateDictionaries(selectQuery, filteredItems);
			} else {
				items = ConvertFilteredItemsToRequestedDictionaries(selectQuery, filteredItems);
			}

			return new ATF.Repository.Mock.Internal.ItemsResponse() {
				Success = true,
				ErrorMessage = null,
				Items = items
			};
		}

		public override IExecuteResponse BatchExecute(List<IBaseQuery> queries) {
			queries.ForEach(query => {
				if (query is IInsertQuery insertQuery) {
					ExecuteInsertQuery(insertQuery);
				}
				if (query is IUpdateQuery updateQuery) {
					ExecuteUpdateQuery(updateQuery);
				}
				if (query is IDeleteQuery deleteQuery) {
					ExecuteDeleteQuery(deleteQuery);
				}
			});
			return new Internal.ExecuteResponse() {
				Success = true
			};
		}

		#endregion

	}

	#endregion

}
