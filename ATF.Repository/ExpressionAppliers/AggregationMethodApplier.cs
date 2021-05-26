using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ATF.Repository.ExpressionConverters;
using ATF.Repository.Queryables;
using Terrasoft.Common;
using Terrasoft.Core.Entities;
using Terrasoft.Nui.ServiceModel.DataContract;

namespace ATF.Repository.ExpressionAppliers
{
	internal class AggregationMethodApplier: ExpressionApplier
	{
		private static readonly Dictionary<string, AggregationType> AggregationTypes = new Dictionary<string, AggregationType>() {
			{"Average", AggregationType.Avg},
			{"Count", AggregationType.Count},
			{"Max", AggregationType.Max},
			{"Min", AggregationType.Min},
			{"Sum", AggregationType.Sum},
		};

		internal override bool Apply(ExpressionChainItem expressionChainItem, ModelQueryBuildConfig config) {
			var converter = new InitialCallExpressionConverter(expressionChainItem.Expression);
			var expressionMetadata = converter.ConvertNode();
			var methodName = expressionChainItem.Expression.Method.Name;
			var aggregationColumnName = RepositoryExpressionUtilities.GetAggregationColumnName(methodName);
			config.SelectQuery.Columns.Items.Clear();
			config.SelectQuery.Columns.Items.Add(aggregationColumnName, GetAggregationColumn(methodName, expressionMetadata));
			return true;
		}

		private static SelectQueryColumn GetAggregationColumn(string methodName, ExpressionMetadata expressionMetadata) {
			return new SelectQueryColumn() {
				Expression = new ColumnExpression() {
					AggregationType = GetAggregationType(methodName),
					ExpressionType = EntitySchemaQueryExpressionType.Function,
					FunctionType = FunctionType.Aggregation,
					FunctionArgument = new ColumnExpression() {
						ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn,
						ColumnPath = GetColumnPath(expressionMetadata)
					}
				}
			};
		}

		private static string GetColumnPath(ExpressionMetadata expressionMetadata) {
			if (expressionMetadata.Items == null || expressionMetadata.Items.Count != 1) {
				throw new NotSupportedException();
			}
			var node = expressionMetadata.Items.First();
			if (node.NodeType != ExpressionMetadataNodeType.Column) {
				throw new NotSupportedException();
			}
			return node.Parameter.ColumnPath;
		}

		private static AggregationType GetAggregationType(string methodName) {
			if (AggregationTypes.ContainsKey(methodName)) {
				return AggregationTypes[methodName];
			}
			throw new NotSupportedException();
		}

	}
}
