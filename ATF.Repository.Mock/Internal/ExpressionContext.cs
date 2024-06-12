namespace ATF.Repository.Mock.Internal
{
	using System.Data;
	using System.Linq.Expressions;

	#region Class: ExpressionBuilder

	internal class ExpressionContext
	{

		#region Fields: Private

		private readonly int _level;

		#endregion

		#region Properties: Internal

		internal DataTable ContextTable { get; private set; }
		internal ParameterExpression RowExpression { get; set; }

		#endregion

		#region Constuctors: Internal

		internal ExpressionContext(DataTable dataTable, int level = 0) {
			_level = level;
			ContextTable = dataTable;
			RowExpression = Expression.Parameter(typeof(DataRow), $"row{_level}");
		}

		#endregion

		#region Methods: Internal

		internal ExpressionContext GetNestedExpressionContext(DataTable dataTable) {
			return new ExpressionContext(dataTable, _level + 1);
		}

		#endregion

	}

	#endregion

}
