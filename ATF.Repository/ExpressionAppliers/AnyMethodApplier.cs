namespace ATF.Repository.ExpressionAppliers
{
	using ATF.Repository.ExpressionConverters;
	using ATF.Repository.Queryables;
	using ATF.Repository.Replicas;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;
	using FunctionType = Terrasoft.Nui.ServiceModel.DataContract.FunctionType;

	internal class AnyMethodApplier: WhereMethodApplier
	{
		internal override bool Apply(ExpressionMetadataChainItem expressionMetadataChainItem, ModelQueryBuildConfig config) {
			if (expressionMetadataChainItem.Expression.Arguments.Count > 1 &&
			    !base.Apply(expressionMetadataChainItem, config)) {
				return false;
			}
			var aggregationColumnName = RepositoryExpressionUtilities.GetAnyColumnName();
			config.SelectQuery.Columns.Items.Clear();
			config.SelectQuery.Columns.Items.Add(aggregationColumnName, new SelectQueryColumnReplica() {
				Expression = new ColumnExpressionReplica() {
					AggregationType = AggregationType.Count,
					ExpressionType = EntitySchemaQueryExpressionType.Function,
					FunctionType = FunctionType.Aggregation,
					FunctionArgument = new ColumnExpressionReplica() {
						ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn,
						ColumnPath = "Id"
					}
				}
			});
			return true;
		}
	}
}
