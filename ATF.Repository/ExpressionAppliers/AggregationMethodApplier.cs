namespace ATF.Repository.ExpressionAppliers
{
	using System;
	using System.Collections.Generic;
	using ATF.Repository.ExpressionConverters;
	using ATF.Repository.Queryables;
	using ATF.Repository.Replicas;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;
	using FunctionType = Terrasoft.Nui.ServiceModel.DataContract.FunctionType;

	internal class AggregationMethodApplier: ExpressionApplier
	{
		private static readonly Dictionary<string, AggregationType> AggregationTypes = new Dictionary<string, AggregationType>() {
			{"Average", AggregationType.Avg},
			{"Count", AggregationType.Count},
			{"Max", AggregationType.Max},
			{"Min", AggregationType.Min},
			{"Sum", AggregationType.Sum},
		};

		internal override bool Apply(ExpressionMetadataChainItem expressionMetadataChainItem, ModelQueryBuildConfig config) {
			var methodName = expressionMetadataChainItem.Expression.Method.Name;
			var aggregationColumnName = RepositoryExpressionUtilities.GetAggregationColumnName(methodName);
			config.SelectQuery.Columns.Items.Clear();
			config.SelectQuery.Columns.Items.Add(aggregationColumnName, GetAggregationColumn(methodName, expressionMetadataChainItem.ExpressionMetadata.Parameter.ColumnPath));
			return true;
		}

		private static SelectQueryColumnReplica GetAggregationColumn(string methodName, string columnPath) {
			return new SelectQueryColumnReplica() {
				Expression = new ColumnExpressionReplica() {
					AggregationType = GetAggregationType(methodName),
					ExpressionType = EntitySchemaQueryExpressionType.Function,
					FunctionType = FunctionType.Aggregation,
					FunctionArgument = new ColumnExpressionReplica() {
						ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn,
						ColumnPath = columnPath
					}
				}
			};
		}

		private static AggregationType GetAggregationType(string methodName) {
			if (AggregationTypes.ContainsKey(methodName)) {
				return AggregationTypes[methodName];
			}
			throw new NotSupportedException();
		}
	}
}
