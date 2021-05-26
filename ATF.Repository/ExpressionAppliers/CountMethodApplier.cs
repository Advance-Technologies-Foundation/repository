using System.Linq.Expressions;
using ATF.Repository.Queryables;
using Terrasoft.Common;
using Terrasoft.Core.Entities;
using Terrasoft.Nui.ServiceModel.DataContract;

namespace ATF.Repository.ExpressionAppliers
{
	internal class CountMethodApplier: WhereMethodApplier
	{
		internal override bool Apply(ExpressionChainItem expressionChainItem, ModelQueryBuildConfig config) {
			if (expressionChainItem.Expression.Arguments.Count > 1 && !base.Apply(expressionChainItem, config)) {
				return false;
			}
			var methodName = expressionChainItem.Expression.Method.Name;
			var aggregationColumnName = RepositoryExpressionUtilities.GetAggregationColumnName(methodName);
			config.SelectQuery.Columns.Items.Clear();
			config.SelectQuery.Columns.Items.Add(aggregationColumnName, new SelectQueryColumn() {
				Expression = new ColumnExpression() {
					AggregationType = AggregationType.Count,
					ExpressionType = EntitySchemaQueryExpressionType.Function,
					FunctionType = FunctionType.Aggregation,
					FunctionArgument = new ColumnExpression() {
						ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn,
						ColumnPath = "Id"
					}
				}
			});
			return true;
		}
	}
}
