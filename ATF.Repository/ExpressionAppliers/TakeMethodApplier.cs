using System;
using System.Linq;
using System.Linq.Expressions;
using ATF.Repository.Queryables;

namespace ATF.Repository.ExpressionAppliers
{
	internal class TakeMethodApplier : ExpressionApplier
	{
		internal override bool Apply(ExpressionChainItem expressionChainItem, ModelQueryBuildConfig config) {
			config.SelectQuery.RowCount = Math.Min(ModelQueryBuildConfig.MaxRowsCount, GetRowsCountAmount(expressionChainItem.Expression));
			return true;
		}

		private static int GetRowsCountAmount(MethodCallExpression expression) {
			var arguments = expression.Arguments;
			var rowOffsetExpression = arguments.Skip(1).FirstOrDefault();
			if (rowOffsetExpression != null && RepositoryExpressionUtilities.TryDynamicInvoke(rowOffsetExpression, out var value)) {
				return (int)value;
			}
			throw new NotImplementedException();
		}
	}
}
