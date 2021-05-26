namespace ATF.Repository.Queryables
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using ATF.Repository.Providers;

	internal class ModelQuery<T>: IOrderedQueryable<T>, IEnumerable<T>
	{
		#region Fields: Private

		private readonly IDataProvider _dataProvider;

		#endregion

		#region Properties: Public

		public Expression Expression { get; private set; }
		public Type ElementType { get; private set; } = typeof(T);

		public ModelQueryProvider Provider { get; set; }

		IQueryProvider IQueryable.Provider => Provider;

		#endregion

		internal ModelQuery(IDataProvider dataProvider, AppDataContext appDataContext) {
			_dataProvider = dataProvider;
			Expression = Expression.Constant(this);
			Provider = new ModelQueryProvider(dataProvider, appDataContext, ElementType);
		}
		internal ModelQuery(IDataProvider dataProvider, ModelQueryProvider queryProvider, Expression expression) {
			_dataProvider = dataProvider;
			Expression = expression;
			Provider = queryProvider;
		}

		#region Methods: Public

		public IEnumerator<T> GetEnumerator() {
			return Provider.ExecuteEnumerable<T>(Expression).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return Provider.ExecuteEnumerable(ElementType, Expression).GetEnumerator();
		}


		#endregion


	}
}
