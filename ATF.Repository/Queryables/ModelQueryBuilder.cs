namespace ATF.Repository.Queryables
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.ExpressionAppliers;
	using ATF.Repository.ExpressionConverters;
	using ATF.Repository.Replicas;
	using Terrasoft.Common;
	using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;

	internal class ModelQueryBuildConfig
	{
		internal SelectQueryReplica SelectQuery { get; set; }
		internal Type ModelType { get; set; }

		internal static int MaxRowsCount = 20000;
	}
	internal class ModelQueryBuilder
	{
		public static SelectQueryReplica BuildEmptyQuery(Type elementType) {
			var schemaName = ModelUtilities.GetSchemaName(elementType);
			return new SelectQueryReplica() {
				RootSchemaName = schemaName,
				RowCount = ModelQueryBuildConfig.MaxRowsCount,
				Columns = new SelectQueryColumnsReplica(),
				Filters = new FilterGroupReplica(),
				IgnoreDisplayValues = true
			};
		}

		public static SelectQueryReplica BuildSelectQuery(ExpressionMetadataChain expressionMetadataChain) {
			if (expressionMetadataChain.IsEmpty()) {
				var defaultConfig = GenerateModelQueryBuildConfig(expressionMetadataChain.LastValueType, hasSelect: false, hasGroupBy: false);
				return defaultConfig.SelectQuery;
			}

			var modelType = expressionMetadataChain.GetModelType();
			var hasSelect = expressionMetadataChain.Items.Any(x =>
				x.Expression.Method.Name == ConvertableExpressionMethod.Select);

			var hasGroupBy = expressionMetadataChain.Items.Any(x =>
				x.Expression.Method.Name == ConvertableExpressionMethod.GroupBy);

			var config = GenerateModelQueryBuildConfig(modelType, hasSelect, hasGroupBy);
			expressionMetadataChain.Items.TakeWhile(x => ApplyExpressionChainItemOnSelectQuery(x, config)).ToList();
			OptimizeFilters(config.SelectQuery.Filters);
			config.SelectQuery.IsPageable = config.SelectQuery.RowsOffset > 0;
			return config.SelectQuery;
		}

		private static ModelQueryBuildConfig GenerateModelQueryBuildConfig(Type modelType, bool hasSelect, bool hasGroupBy) {
			var config = new ModelQueryBuildConfig() {
				ModelType = modelType,
				SelectQuery = BuildEmptyQuery(modelType)
			};
			if (!hasSelect && !hasGroupBy) {
				ExpressionApplier.AddAllColumns(config);
			}
			return config;
		}

		private static void OptimizeFilters(IFilter filter) {
			if (filter.FilterType != FilterType.FilterGroup) {
				return;
			}

			var filtersToMove = new List<IFilter>();
			var itemsToDelete = new List<string>();
			filter.Items.ForEach(nestedFilterItem => {
				var nestedFilter = nestedFilterItem.Value;
				OptimizeFilters(nestedFilter);

				if (nestedFilter.FilterType == filter.FilterType &&
				    nestedFilter.LogicalOperation == filter.LogicalOperation) {
					nestedFilter.Items.ForEach(nfi => filtersToMove.Add(nfi.Value));
					itemsToDelete.Add(nestedFilterItem.Key);
				}
			});
			filtersToMove.ForEach(f => filter.Items.Add(Guid.NewGuid().ToString(), f));
			itemsToDelete.ForEach(key => filter.Items.Remove(key));

		}

		private static bool ApplyExpressionChainItemOnSelectQuery(ExpressionMetadataChainItem expressionChainItem, ModelQueryBuildConfig selectQuery) {
			var methodName = expressionChainItem.Expression.Method.Name;
			var expressionApplier = ExpressionApplier.GetApplier(methodName);
			var appliedResult = expressionApplier?.Apply(expressionChainItem, selectQuery) ?? false;
			expressionChainItem.IsAppliedToQuery = appliedResult;
			return expressionChainItem.IsAppliedToQuery;
		}

	}


}
