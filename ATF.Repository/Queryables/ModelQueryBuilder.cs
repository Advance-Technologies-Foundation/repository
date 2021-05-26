using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ATF.Repository.ExpressionAppliers;
using ATF.Repository.ExpressionConverters;
using ATF.Repository.Mapping;
using Terrasoft.Common;
using Terrasoft.Core.Entities;
using Terrasoft.Nui.ServiceModel.DataContract;
using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;

namespace ATF.Repository.Queryables
{
	internal class ModelQueryBuildConfig
	{
		internal SelectQuery SelectQuery { get; set; }
		internal Type ModelType { get; set; }

		internal static int MaxRowsCount = 100;
	}
	internal class ModelQueryBuilder
	{
		public static SelectQuery BuildEmptyQuery(Type elementType) {
			var schemaName = ModelUtilities.GetSchemaName(elementType);
			return new SelectQuery() {
				RootSchemaName = schemaName,
				RowCount = ModelQueryBuildConfig.MaxRowsCount,
				Columns = new SelectQueryColumns() {Items = new Dictionary<string, SelectQueryColumn>()},
				Filters = new Filters() {Items = new Dictionary<string, Filter>(), FilterType = FilterType.FilterGroup}
			};
		}

		public static SelectQuery BuildSelectQuery(ExpressionChain expressionChain, Type finalElementType) {
			if (!expressionChain.Any()) {
				var defaultConfig = GenerateModelQueryBuildConfig(finalElementType);
				return defaultConfig.SelectQuery;
			}

			var modelType = expressionChain.GetModelType();
			var config = GenerateModelQueryBuildConfig(modelType);
			expressionChain.OrderBy(x=>x.Position).TakeWhile(x => ApplyExpressionChainItemOnSelectQuery(x, config)).ToList();
			OptimizeFilters(config.SelectQuery.Filters);
			return config.SelectQuery;
		}

		private static ModelQueryBuildConfig GenerateModelQueryBuildConfig(Type modelType) {
			var config = new ModelQueryBuildConfig() {
				ModelType = modelType,
				SelectQuery = BuildEmptyQuery(modelType)
			};
			ExpressionApplier.AddAllColumns(config);
			return config;
		}

		private static void OptimizeFilters(Filter filter) {
			if (filter.FilterType != FilterType.FilterGroup) {
				return;
			}

			var filtersToMove = new List<Filter>();
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

		private static bool ApplyExpressionChainItemOnSelectQuery(ExpressionChainItem expressionChainItem, ModelQueryBuildConfig selectQuery) {
			var expressionApplier = ExpressionApplier.GetApplier(expressionChainItem.Expression.Method.Name);
			expressionChainItem.IsAppliedToQuery = expressionApplier?.Apply(expressionChainItem, selectQuery) ?? false;
			return expressionChainItem.IsAppliedToQuery;
		}

	}


}
