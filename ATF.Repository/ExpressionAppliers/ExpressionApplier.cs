using System;
using System.Collections.Generic;
using System.Linq;
using ATF.Repository.ExpressionConverters;
using ATF.Repository.Mapping;
using ATF.Repository.Queryables;
using Terrasoft.Common;
using Terrasoft.Core.Entities;
using Terrasoft.Nui.ServiceModel.DataContract;

namespace ATF.Repository.ExpressionAppliers
{
	internal abstract class ExpressionApplier
	{
		private static readonly Dictionary<string, Type> Appliers = new Dictionary<string, Type>() {
			{"Skip", typeof(SkipMethodApplier)},
			{"Take", typeof(TakeMethodApplier)},
			{"FirstOrDefault", typeof(FirstMethodApplier)},
			{"First", typeof(FirstMethodApplier)},
			{"Where", typeof(WhereMethodApplier)},
			{"OrderBy", typeof(OrderMethodApplier)},
			{"OrderByDescending", typeof(OrderMethodApplier)},
			{"ThenBy", typeof(OrderMethodApplier)},
			{"ThenByDescending", typeof(OrderMethodApplier)},
			{"Max", typeof(AggregationMethodApplier)},
			{"Min", typeof(AggregationMethodApplier)},
			{"Average", typeof(AggregationMethodApplier)},
			{"Sum", typeof(AggregationMethodApplier)},
			{"Count", typeof(CountMethodApplier)},
			{"Any", typeof(AnyMethodApplier)},
		};

		internal abstract bool Apply(ExpressionMetadataChainItem expression, ModelQueryBuildConfig config);


		protected SelectQueryColumn GetOrAddColumn(ModelQueryBuildConfig config, string columnPath) {
			AddColumnIfNotExists(config, columnPath);
			return config.SelectQuery.Columns.Items[columnPath];
		}

		internal static ExpressionApplier GetApplier(string methodName) {
			if (Appliers.ContainsKey(methodName)) {
				return (ExpressionApplier) Activator.CreateInstance(Appliers[methodName]);
			}
			return null;
		}

		internal static void AddAllColumns(ModelQueryBuildConfig config) {
			var columnsItems = config.SelectQuery.Columns.Items;
			var generatedColumns = GenerateColumns(config);
			generatedColumns.ForEach(item => {
				if (!columnsItems.ContainsKey(item.Key)) {
					columnsItems.Add(item.Key, item.Value);
				}
			});
		}

		private static void AddColumnIfNotExists(ModelQueryBuildConfig config, string columnPath) {
			if (!config.SelectQuery.Columns.Items.ContainsKey(columnPath)) {
				config.SelectQuery.Columns.Items.Add(columnPath, GenerateSelectQueryColumn(columnPath));
			}
		}

		private static Dictionary<string, SelectQueryColumn> GenerateColumns(ModelQueryBuildConfig config) {
			var columns = new Dictionary<string, SelectQueryColumn>();
			ModelMapper.GetModelItems(config.ModelType).Where(modelItem =>
					modelItem.PropertyType == ModelItemType.Column || modelItem.PropertyType == ModelItemType.Lookup)
				.ForEach(property => {
					if (!columns.ContainsKey(property.EntityColumnName)) {
						columns.Add(property.EntityColumnName, GenerateSelectQueryColumn(property));
					}
				});
			return columns;
		}

		private static SelectQueryColumn GenerateSelectQueryColumn(ModelItem property) {
			return GenerateSelectQueryColumn(property.EntityColumnName);
		}

		private static SelectQueryColumn GenerateSelectQueryColumn(string columnPath) {
			return new SelectQueryColumn() {
				Expression = new ColumnExpression() {
					ColumnPath = columnPath,
					ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn
				},
				OrderDirection = OrderDirection.None,
				OrderPosition = -1
			};
		}
	}

}
