namespace ATF.Repository.Queryables
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Linq.Expressions;
	using ATF.Repository.ExpressionAppliers;
	using Terrasoft.Common;
	using ATF.Repository.Providers;

	internal class ModelQueryProvider: ExpressionVisitor, IQueryProvider
	{
		private readonly IDataProvider _dataProvider;
		private readonly AppDataContext _appDataContext;
		private readonly Type _elementType;

		private ExpressionChain _expressionChain;
		private ExpressionChain ExpressionChain => _expressionChain ?? (_expressionChain = new ExpressionChain());

		internal ModelQueryProvider(IDataProvider dataProvider, AppDataContext appDataContext, Type elementType) {
			_dataProvider = dataProvider;
			_appDataContext = appDataContext;
			_elementType = elementType;
		}

		public IEnumerable<T> ExecuteEnumerable<T>(Expression expression) {
			var dataCollection = LoadDataCollection();
			var models = LoadModelCollection(dataCollection);
			return ApplyCollectionProjector<T>(models.ToList<object>());
		}

		private IEnumerable<T> ApplyCollectionProjector<T>(IReadOnlyCollection<object> sourceItems) {
			var notAppliedChainItems = ExpressionChain.Where(x => !x.IsAppliedToQuery).OrderBy(x => x.Position).ToList();
			if (notAppliedChainItems.Any()) {
				var method = RepositoryReflectionUtilities.GetGenericMethod(this.GetType(), "ApplyTypedCollectionProjector", typeof(T),
					notAppliedChainItems.First().InputDtoType.Type);
				return (IEnumerable<T>)method.Invoke(this, new object[] {sourceItems});
			}
			return sourceItems.Select(x => (T) x).AsEnumerable();
		}

		private IEnumerable<T> ApplyTypedCollectionProjector<T, TItem>(IEnumerable<object> rawSourceItems) {
			var sourceItems = rawSourceItems.Select(x => (TItem) x).ToList();
			Expression sourceExpression = Expression.Constant(sourceItems.AsQueryable());
			ExpressionChain.Where(x=>!x.IsAppliedToQuery).OrderBy(x=>x.Position).ForEach(x => {
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
			AddExpressionToChain(expression);
			return new ModelQuery<TElement>(_dataProvider, this, expression);
		}

		private void AddExpressionToChain(Expression expression) {
			var item = MakeExpressionChainItem(expression);
			if (item != null) {
				item.Position = ExpressionChain.Count;
				ExpressionChain.Add(item);
			}
		}

		private ExpressionChainItem MakeExpressionChainItem(Expression expression) {
			if (!(expression is MethodCallExpression methodCallExpression)) {
				throw new InvalidTypeCastException(nameof(expression), typeof(MethodCallExpression));
			}
			return new ExpressionChainItem(methodCallExpression);
		}

		public object Execute(Expression expression) {
			throw new System.NotSupportedException();
		}

		public TResult Execute<TResult>(Expression expression) {
			AddExpressionToChain(expression);
			return ExecuteScalar<TResult>(expression);
		}

		private List<BaseModel> LoadModelCollection(List<Dictionary<string, object>> dataCollection) {
			var method = RepositoryReflectionUtilities.GetGenericMethod(GetType(),
				"LoadTypedModelCollection", ExpressionChain.Any() ? ExpressionChain.GetModelType() : _elementType);
			var models = (List<BaseModel>)method?.Invoke(this, new object[] {dataCollection});
			return models;
		}

		private List<BaseModel> LoadTypedModelCollection<T>(List<Dictionary<string, object>> dataCollection) where T: BaseModel, new() {
			var models = _appDataContext.GetModelsByDataCollection<T>(dataCollection);
			return models.Select(x=>(BaseModel)x).ToList();
		}

		private List<Dictionary<string, object>> LoadDataCollection() {
			var selectQuery = ModelQueryBuilder.BuildSelectQuery(ExpressionChain, _elementType);
			var response = _dataProvider.GetItems(selectQuery);
			return response != null && response.Success
				? response.Items
				: new List<Dictionary<string, object>>();
		}

		private T ExecuteScalar<T>(Expression expression) {
			var dataCollection = LoadDataCollection();
			if (ExpressionChain.GetModelType() == ExpressionChain.OutputAppliedType()) {
				var models = LoadModelCollection(dataCollection);
				return ApplyScalarProjector<T>(models.ToList<object>());
			}

			if (RepositoryExpressionUtilities.IsAggregationMethodExpression(expression)) {
				return GetAggregationValue<T>(expression, dataCollection);
			}

			if (RepositoryExpressionUtilities.IsAnyMethodExpression(expression)) {
				return GetAnyValue<T>(dataCollection);
			}

			throw new NotImplementedException();
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
				throw new NullOrEmptyException();
			}

			if (dataCollection.Count > 1) {
				throw new IndexOutOfRangeException();
			}

			var data = dataCollection.First();
			if (!data.ContainsKey(columnName)) {
				throw new NullOrEmptyException();
			}

			var value = data[columnName];
			if (value == null) {
				throw new NullOrEmptyException();
			}

			if (value is T typedValue) {
				return typedValue;
			}
			var converter = TypeDescriptor.GetConverter(data[columnName].GetType());
			if (!converter.CanConvertTo(typeof(T))) {
				throw new InvalidCastException();
			}

			return (T)converter.ConvertTo(value, typeof(T));
		}

		private T ApplyScalarProjector<T>(List<object> sourceItems) {
			var notAppliedChainItems = ExpressionChain.Where(x => !x.IsAppliedToQuery).OrderBy(x => x.Position).ToList();
			if (!notAppliedChainItems.Any()) {
				return sourceItems.Any() ? (T) sourceItems.First() : default(T);
			}
			var method = RepositoryReflectionUtilities.GetGenericMethod(this.GetType(), "ApplyTypedScalarProjector", typeof(T),
				notAppliedChainItems.First().InputDtoType.Type);
			return (T)method.Invoke(this, new object[] {sourceItems});

		}

		private T ApplyTypedScalarProjector<T, TItem>(List<object> rawSourceItems) {
			var sourceItems = rawSourceItems.Select(x => (TItem) x).ToList();
			Expression sourceExpression = Expression.Constant(sourceItems.AsQueryable());
			ExpressionChain.Where(x=>!x.IsAppliedToQuery).OrderBy(x=>x.Position).ForEach(x => {
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
