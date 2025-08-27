namespace ATF.Repository.Mock
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.Mock.Internal;
	using ATF.Repository.Providers;
	using Terrasoft.Common;

	#region Class: DataProviderMock

	public class DataProviderMock: BaseDataProviderMock, IDataProvider
	{

		#region Fields: Private

		private readonly Dictionary<string, DefaultValuesMock> _defaultValuesMocks = new Dictionary<string, DefaultValuesMock>();
		private readonly List<ItemsMock> _collectionItemsMocks = new List<ItemsMock>();
		private readonly List<ScalarMock> _scalarItemsMocks = new List<ScalarMock>();
		private readonly List<MockSavingItem> _batchItemMocks = new List<MockSavingItem>();

		#endregion

		#region Methods: Private 

		private BaseMock GetScalarMock(ISelectQuery selectQuery) {
			if (selectQuery.Columns.Items.Count() != 1) {
				return null;
			}

			var column = selectQuery.Columns.Items.First();
			if (column.Value.Expression.AggregationType == AggregationType.None) {
				return null;
			}
			var queryParameters = QueryParametersExtractor.ExtractParameters(selectQuery);
			return _scalarItemsMocks.OrderBy(x=>x.Position).Where(x=>x.Enabled).FirstOrDefault(x =>
				x.SchemaName == selectQuery.RootSchemaName && x.CheckByParameters(queryParameters));
		}

		private BaseMock GetCollectionMock(ISelectQuery selectQuery) {
			var queryParameters = QueryParametersExtractor.ExtractParameters(selectQuery);
			return _collectionItemsMocks.OrderBy(x=>x.Position).Where(x=>x.Enabled).FirstOrDefault(x =>
				x.SchemaName == selectQuery.RootSchemaName && x.CheckByParameters(queryParameters));
		}

		private void ReceiveBatchQueryItem(IBaseQuery query) {
			if (query is IInsertQuery insertQuery) {
				ReceiveBatchQueryItem(insertQuery);
			} else if (query is IUpdateQuery updateQuery) {
				ReceiveBatchQueryItem(updateQuery);
			} else if (query is IDeleteQuery deleteQuery) {
				ReceiveBatchQueryItem(deleteQuery);
			} else {
				throw new NotSupportedException();
			}
		}

		private void ReceiveBatchQueryItem(IInsertQuery query) {
			var columnValueParameters = QueryParametersExtractor.ExtractColumnValues(query);
			var mock = _batchItemMocks.FirstOrDefault(x =>
				x.SchemaName == query.RootSchemaName && x.Operation == SavingOperation.Insert &&
				x.CheckByColumnValues(columnValueParameters));
			mock?.OnReceived();
		}

		private void ReceiveBatchQueryItem(IUpdateQuery query) {
			var columnValueParameters = QueryParametersExtractor.ExtractColumnValues(query);
			var queryParameters = QueryParametersExtractor.ExtractParameters(query);
			var mock = _batchItemMocks.FirstOrDefault(x =>
				x.SchemaName == query.RootSchemaName &&
				x.Operation == SavingOperation.Update && x.CheckByColumnValues(columnValueParameters) &&
				x.CheckByParameters(queryParameters));
			mock?.OnReceived();
		}

		private void ReceiveBatchQueryItem(IDeleteQuery query) {
			var queryParameters = QueryParametersExtractor.ExtractParameters(query);
			var mock = _batchItemMocks.FirstOrDefault(x =>
				x.SchemaName == query.RootSchemaName && x.Operation == SavingOperation.Delete &&
				x.CheckByParameters(queryParameters));
			mock?.OnReceived();
		}

		#endregion

		#region Methods: Public

		public IDefaultValuesMock MockDefaultValues(string schemaName) {
			if (!_defaultValuesMocks.ContainsKey(schemaName)) {
				_defaultValuesMocks.Add(schemaName, new DefaultValuesMock(schemaName));
			}
			return _defaultValuesMocks[schemaName];
		}

		public override IDefaultValuesResponse GetDefaultValues(string schemaName) {
			if (!_defaultValuesMocks.ContainsKey(schemaName)) {
				return null;
			}
			var mock = _defaultValuesMocks[schemaName];
			mock.OnReceived();
			return mock.GetDefaultValues();
		}

		public IItemsMock MockItems(string schemaName) {
			var mock = new ItemsMock(schemaName) { Position = _collectionItemsMocks.Count };
			_collectionItemsMocks.Add(mock);
			return mock;
		}

		public IScalarMock MockScalar(string schemaName, AggregationScalarType aggregationType) {
			var mock = new ScalarMock(schemaName, aggregationType) { Position = _scalarItemsMocks.Count };
			_scalarItemsMocks.Add(mock);
			return mock;
		}

		public override IItemsResponse GetItems(ISelectQuery selectQuery) {
			var mock = GetScalarMock(selectQuery) ?? GetCollectionMock(selectQuery);
			if (mock == null) {
				return null;
			}
			mock.OnReceived();
			return mock.GetItemsResponse();
		}

		public override IExecuteResponse BatchExecute(List<IBaseQuery> queries) {
			queries.ForEach(ReceiveBatchQueryItem);
			return new ATF.Repository.Mock.Internal.ExecuteResponse() {
				Success = true,
				ErrorMessage = string.Empty,
				QueryResults = new List<IExecuteItemResponse>()
			};
		}

		public IMockSavingItem MockSavingItem(string entitySchema, SavingOperation operation) {
			var mock = new MockSavingItem(entitySchema, operation);
			_batchItemMocks.Add(mock);
			return mock;
		}

		#endregion

	}

	#endregion

}
