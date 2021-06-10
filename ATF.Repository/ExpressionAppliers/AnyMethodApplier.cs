using System.Linq.Expressions;
using ATF.Repository.ExpressionConverters;
using ATF.Repository.Queryables;
using Terrasoft.Common;
using Terrasoft.Core.Entities;
using Terrasoft.Nui.ServiceModel.DataContract;

namespace ATF.Repository.ExpressionAppliers
{
	internal class AnyMethodApplier: WhereMethodApplier
	{

		internal override bool Apply(ExpressionMetadataChainItem expressionMetadataChainItem, ModelQueryBuildConfig config) {
			if (expressionMetadataChainItem.Expression.Arguments.Count > 1 &&
			    !base.Apply(expressionMetadataChainItem, config)) {
				return false;
			}
			var aggregationColumnName = RepositoryExpressionUtilities.GetAnyColumnName();
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
