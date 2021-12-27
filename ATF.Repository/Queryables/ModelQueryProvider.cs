namespace ATF.Repository.Queryables
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Linq.Expressions;
	using ATF.Repository.ExpressionAppliers;
	using ATF.Repository.ExpressionConverters;
	using Terrasoft.Common;
	using ATF.Repository.Providers;

	internal class ModelQueryProvider: ExpressionVisitor, IQueryProvider
	{
		private readonly IDataProvider _dataProvider;
		private readonly AppDataContext _appDataContext;
		private readonly Type _elementType;

		internal ModelQueryProvider(IDataProvider dataProvider, AppDataContext appDataContext, Type elementType) {
			_dataProvider = dataProvider;
			_appDataContext = appDataContext;
			_elementType = elementType;
		}

		public IEnumerable<T> ExecuteEnumerable<T>(Expression expression) {
			var chain = ExpressionToMetadataConverter.Convert(expression, _elementType);
			var dataCollection = LoadDataCollection(chain);
			var models = LoadModelCollection(dataCollection, chain);
			return ApplyCollectionProjector<T>(models.ToList<object>(), chain);
		}

		private IEnumerable<T> ApplyCollectionProjector<T>(IReadOnlyCollection<object> sourceItems, ExpressionMetadataChain chain) {
			var notAppliedChainItems = chain.Items.Where(x => !x.IsAppliedToQuery).ToList();
			if (notAppliedChainItems.Any()) {
				var method = RepositoryReflectionUtilities.GetGenericMethod(this.GetType(), "ApplyTypedCollectionProjector", typeof(T),
					notAppliedChainItems.First().InputDtoType.Type);
				return (IEnumerable<T>)method.Invoke(this, new object[] {sourceItems, chain});
			}
			return sourceItems.Select(x => (T) x).AsEnumerable();
		}

		private IEnumerable<T> ApplyTypedCollectionProjector<T, TItem>(IEnumerable<object> rawSourceItems, ExpressionMetadataChain chain) {
			var sourceItems = rawSourceItems.Select(x => (TItem) x).ToList();
			Expression sourceExpression = Expression.Constant(sourceItems.AsQueryable());
			chain.Items.Where(x=>!x.IsAppliedToQuery).ForEach(x => {
				sourceExpression = Expression.Call(null, x.Expression.Method, sourceExpression,
					x.Expression.Arguments.Skip(1).First());
			});

			return (IEnumerable<T>)Expression.Lambda(sourceExpression).Compile().DynamicInvoke();
		}

		public IEnumerable ExecuteEnumerable(Type type, Expression expression) {
			var method = RepositoryReflectionUtilities.GetGenericMethod(this.GetType(), "ExecuteEnumerable", type);
			return (IEnumerable)method.Invoke(this, new object[] {expression});
		}

		public IQueryable CreateQuery(Expression expression) {
			throw new System.NotSupportedException();
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression) {
			expression.CheckArgumentNull(nameof(expression));
			return new ModelQuery<TElement>(_dataProvider, this, expression);
		}

		public object Execute(Expression expression) {
			throw new System.NotSupportedException();
		}

		public TResult Execute<TResult>(Expression expression) {
			return ExecuteScalar<TResult>(expression);
		}

		private List<BaseModel> LoadModelCollection(List<Dictionary<string, object>> dataCollection, ExpressionMetadataChain chain) {
			var method = RepositoryReflectionUtilities.GetGenericMethod(GetType(),
				"LoadTypedModelCollection", chain.GetModelType());
			var models = (List<BaseModel>)method?.Invoke(this, new object[] {dataCollection});
			return models;
		}

		private List<BaseModel> LoadTypedModelCollection<T>(List<Dictionary<string, object>> dataCollection) where T: BaseModel, new() {
			var models = _appDataContext.GetModelsByDataCollection<T>(dataCollection);
			return models.Select(x=>(BaseModel)x).ToList();
		}

		private List<Dictionary<string, object>> LoadDataCollection(ExpressionMetadataChain chain) {
			var selectQuery = ModelQueryBuilder.BuildSelectQuery(chain);
			var response = _dataProvider.GetItems(selectQuery);
			return response != null && response.Success
				? response.Items
				: new List<Dictionary<string, object>>();
		}

		private T ExecuteScalar<T>(Expression expression) {
			var chain = ExpressionToMetadataConverter.Convert(expression, _elementType);
			var dataCollection = LoadDataCollection(chain);
			if (RepositoryExpressionUtilities.IsAggregationMethodExpression(expression)) {
				return GetAggregationValue<T>(expression, dataCollection);
			}
			if (RepositoryExpressionUtilities.IsAnyMethodExpression(expression)) {
				return GetAnyValue<T>(dataCollection);
			}
			var models = LoadModelCollection(dataCollection, chain);
			return ApplyScalarProjector<T>(models.ToList<object>(), chain);
		}

		private static T GetAnyValue<T>(IReadOnlyCollection<Dictionary<string, object>>
			dataCollection) {
			var columnName = RepositoryExpressionUtilities.GetAnyColumnName();
			var count = GetAggregationValue<int>(columnName, dataCollection);
			var value = count > 0;
			if (value is T typedValue) {
				return typedValue;
			}

			return default(T);
		}

		private static T GetAggregationValue<T>(Expression expression, IReadOnlyCollection<Dictionary<string, object>> dataCollection) {
			var methodName = RepositoryExpressionUtilities.GetMethodName(expression);
			var columnName = RepositoryExpressionUtilities.GetAggregationColumnName(methodName);
			return GetAggregationValue<T>(columnName, dataCollection);
		}

		private static T GetAggregationValue<T>(string columnName, IReadOnlyCollection<Dictionary<string, object>> dataCollection) {
			if (!dataCollection.Any()) {
				return default(T);
			}

			var data = dataCollection.First();
			if (!data.ContainsKey(columnName)) {
				return default(T);
			}

			var value = data[columnName];
			if (value == null) {
				return default(T);
			}

			if (value is T typedValue) {
				return typedValue;
			}
			var converter = TypeDescriptor.GetConverter(data[columnName].GetType());
			if (!converter.CanConvertTo(typeof(T))) {
				return default(T);
			}

			return (T)converter.ConvertTo(value, typeof(T));
		}

		private T ApplyScalarProjector<T>(List<object> sourceItems, ExpressionMetadataChain chain) {
			var notAppliedChainItems = chain.Items.Where(x => !x.IsAppliedToQuery).ToList();
			if (!notAppliedChainItems.Any()) {
				return sourceItems.Any() ? (T) sourceItems.First() : default(T);
			}
			var method = RepositoryReflectionUtilities.GetGenericMethod(this.GetType(), "ApplyTypedScalarProjector", typeof(T),
				notAppliedChainItems.First().InputDtoType.Type);
			return (T)method.Invoke(this, new object[] {sourceItems, chain});

		}

		private T ApplyTypedScalarProjector<T, TItem>(List<object> rawSourceItems, ExpressionMetadataChain chain) {
			var sourceItems = rawSourceItems.Select(x => (TItem) x).ToList();
			Expression sourceExpression = Expression.Constant(sourceItems.AsQueryable());
			chain.Items.Where(x=>!x.IsAppliedToQuery).ForEach(x => {
				if (x.Expression.Arguments.Count == 1) {
					sourceExpression = Expression.Call(null, x.Expression.Method, sourceExpression);
				} else if (x.Expression.Arguments.Count == 2) {
					sourceExpression = Expression.Call(null, x.Expression.Method, sourceExpression,
						x.Expression.Arguments.Skip(1).First());
				} else {
					throw new System.NotSupportedException();
				}
			});

			return (T)Expression.Lambda(sourceExpression).Compile().DynamicInvoke();
		}

	}
}
