namespace ATF.Repository.ExpressionAppliers
{
	using System;
	using System.Linq;
	using ATF.Repository.ExpressionConverters;
	using ATF.Repository.Queryables;
	using ATF.Repository.Replicas;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;
	using QuerySource = Terrasoft.Nui.ServiceModel.DataContract.QuerySource;
	using FunctionType = Terrasoft.Nui.ServiceModel.DataContract.FunctionType;
	using DatePart = Terrasoft.Nui.ServiceModel.DataContract.DatePart;

	internal class GroupByMethodApplier : ExpressionApplier
	{
		internal override bool Apply(ExpressionMetadataChainItem expression, ModelQueryBuildConfig config)
		{
			var metadata = expression.ExpressionMetadata;

			if (metadata == null || metadata.NodeType != ExpressionMetadataNodeType.Group || metadata.Items == null || !metadata.Items.Any())
			{
				return false;
			}

			// Set GroupBy-specific query parameters
			config.SelectQuery.IsPageable = false;
			config.SelectQuery.RowsOffset = -1;
			config.SelectQuery.QuerySource = (QuerySource)2;

			// Clear existing columns (GroupBy replaces all columns)
			config.SelectQuery.Columns.Items.Clear();

			// Add columns from GroupBy result selector
			foreach (var item in metadata.Items)
			{
				AddGroupByColumn(item, config);
			}

			return true;
		}

		private void AddGroupByColumn(ExpressionMetadata item, ModelQueryBuildConfig config)
		{
			var columnAlias = item.Code ?? item.Parameter?.ColumnPath;

			if (string.IsNullOrEmpty(columnAlias))
			{
				throw new InvalidOperationException("Column alias cannot be null or empty");
			}

			ColumnExpressionReplica expression;

			switch (item.NodeType)
			{
				case ExpressionMetadataNodeType.Column:
					// Grouping key: expressionType = 0 (SchemaColumn)
					expression = new ColumnExpressionReplica
					{
						ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn,
						ColumnPath = item.Parameter.ColumnPath
					};
					break;

				case ExpressionMetadataNodeType.Function:
					// Check if it's DatePart or Aggregation
					if (item.DatePart != DatePart.None)
					{
						// DatePart function (x.DateTimeValue.Hour)
						expression = CreateDatePartExpression(item);
					}
					else if (!string.IsNullOrEmpty(item.MethodName))
					{
						// Aggregation (items.Count(), items.Sum(), etc.)
						expression = CreateFunctionAggregationExpression(item);
					}
					else
					{
						throw new NotSupportedException($"Unknown Function type in GroupBy");
					}
					break;

				default:
					throw new NotSupportedException($"NodeType {item.NodeType} is not supported in GroupBy projection");
			}

			var column = new SelectQueryColumnReplica
			{
				Expression = expression,
				OrderDirection = OrderDirection.None,
				OrderPosition = -1
			};

			config.SelectQuery.Columns.Items.Add(columnAlias, column);
		}

		private ColumnExpressionReplica CreateDatePartExpression(ExpressionMetadata item)
		{
			// DatePart function: expressionType=1, functionType=1 (DatePart)
			return new ColumnExpressionReplica
			{
				ExpressionType = EntitySchemaQueryExpressionType.Function,
				FunctionType = FunctionType.DatePart,
				DatePartType = item.DatePart,
				FunctionArgument = new ColumnExpressionReplica
				{
					ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn,
					ColumnPath = item.Parameter.ColumnPath
				}
			};
		}

		private ColumnExpressionReplica CreateFunctionAggregationExpression(ExpressionMetadata item)
		{
			// GroupBy aggregations use expressionType = 1 (Function)
			// with functionType = 2 (Aggregation) and functionArgument containing the column
			//
			// Example:
			// "expression": {
			//     "expressionType": 1,      // Function
			//     "functionType": 2,        // Aggregation
			//     "functionArgument": {
			//         "expressionType": 0,  // SchemaColumn
			//         "columnPath": "Id"
			//     },
			//     "aggregationType": 1      // Count
			// }

			var aggregationType = GetAggregationType(item.MethodName);
			var columnPath = item.Parameter?.ColumnPath ?? "Id";

			// Create the function argument (the column being aggregated)
			var functionArgument = new ColumnExpressionReplica
			{
				ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn,
				ColumnPath = columnPath
			};

			return new ColumnExpressionReplica
			{
				ExpressionType = EntitySchemaQueryExpressionType.Function,
				FunctionType = FunctionType.Aggregation,
				AggregationType = aggregationType,
				FunctionArgument = functionArgument
			};
		}

		private AggregationType GetAggregationType(string methodName)
		{
			switch (methodName)
			{
				case ConvertableExpressionMethod.Count:
					return AggregationType.Count;
				case ConvertableExpressionMethod.Sum:
					return AggregationType.Sum;
				case ConvertableExpressionMethod.Max:
					return AggregationType.Max;
				case ConvertableExpressionMethod.Min:
					return AggregationType.Min;
				case ConvertableExpressionMethod.Average:
					return AggregationType.Avg;
				default:
					throw new NotSupportedException($"Aggregation method {methodName} is not supported");
			}
		}
	}
}
