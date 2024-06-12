namespace ATF.Repository.Mock
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq.Expressions;
	using ATF.Repository.Mock.Internal;
	using ATF.Repository.Providers;
	using Terrasoft.Common;

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

		private List<DataRow> GetFilteredItems(ISelectQuery selectQuery) {
			var filteredItems = new List<DataRow>();
			var table = _dataSet.Tables[selectQuery.RootSchemaName];
			var expressionContext = new ExpressionContext(table);
			var filterExpression = ExpressionBuilder.BuildFilter(expressionContext, selectQuery);
			var iterationExpression = (Expression<Func<DataRow, bool>>)filterExpression;
			Func<DataRow, bool> iterationMethod = iterationExpression.Compile();

			foreach (var dataRow in table.AsEnumerable()) {
				if (iterationMethod.Invoke(dataRow)) {
					filteredItems.Add(dataRow);
				}
			}

			return filteredItems;
		}

		#endregion

		#region Methods: Public

		public IDefaultValuesResponse GetDefaultValues(string schemaName) {
			throw new System.NotImplementedException();
		}

		public IItemsResponse GetItems(ISelectQuery selectQuery) {
			List<DataRow> filteredItems = GetFilteredItems(selectQuery);
			List<Dictionary<string, object>> items =
				ConvertFilteredItemsToRequestedDictionaries(selectQuery, filteredItems);
			return new ATF.Repository.Mock.Internal.ItemsResponse() {
				Success = true,
				ErrorMessage = null,
				Items = items
			};
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
