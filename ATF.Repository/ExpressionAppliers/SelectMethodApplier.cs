namespace ATF.Repository.ExpressionAppliers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.ExpressionConverters;
	using ATF.Repository.Queryables;
	using ATF.Repository.Replicas;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;
	using Terrasoft.Nui.ServiceModel.DataContract;
	using DatePart = Terrasoft.Nui.ServiceModel.DataContract.DatePart;

	internal class SelectMethodApplier : ExpressionApplier
	{
		internal override bool Apply(ExpressionMetadataChainItem expression, ModelQueryBuildConfig config)
		{
			var metadata = expression.ExpressionMetadata;

		if (metadata == null || metadata.NodeType != ExpressionMetadataNodeType.Group || metadata.Items == null || !metadata.Items.Any())
		{
			return false;
		}

		// Save ordered columns before clearing (OrderBy/ThenBy columns that must be preserved)
		var orderedColumns = config.SelectQuery.Columns.Items
			.Where(x => x.Value.OrderDirection != OrderDirection.None)
			.ToDictionary(x => x.Key, x => x.Value);

		// Clear existing columns (Select replaces all columns)
		config.SelectQuery.Columns.Items.Clear();

		// Add selected columns (skip constants - they don't go to query)
		foreach (var item in metadata.Items)
		{
			if (item.NodeType == ExpressionMetadataNodeType.Constant)
			{
				// Constants are not added to the query - they're handled in result construction
				continue;
			}

			AddSelectColumn(item, config);
		}

		// Restore ordered columns (needed for OrderBy/ThenBy even if not in Select)
		foreach (var orderedColumn in orderedColumns)
		{
			if (!config.SelectQuery.Columns.Items.ContainsKey(orderedColumn.Key))
			{
				// Column not in Select - add it for ordering
				config.SelectQuery.Columns.Items.Add(orderedColumn.Key, orderedColumn.Value);
			}
			else
			{
				// Column exists in Select - restore ordering info
				var existingColumn = (SelectQueryColumnReplica)config.SelectQuery.Columns.Items[orderedColumn.Key];
				existingColumn.OrderDirection = orderedColumn.Value.OrderDirection;
				existingColumn.OrderPosition = orderedColumn.Value.OrderPosition;
			}
		}

		return true;
		}

		private void AddSelectColumn(ExpressionMetadata item, ModelQueryBuildConfig config)
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
					// Simple column or lookup path
					expression = new ColumnExpressionReplica
					{
						ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn,
						ColumnPath = item.Parameter.ColumnPath
					};
					break;

				case ExpressionMetadataNodeType.Detail:
					// Detail aggregation (based on case1.txt)
					expression = CreateDetailAggregationExpression(item);
					break;

				case ExpressionMetadataNodeType.Function:
					// DatePart function (x.DateTimeValue.Hour)
					if (item.DatePart != DatePart.None)
					{
						expression = CreateDatePartExpression(item);
					}
					else
					{
						throw new NotSupportedException($"Function type is not supported in Select");
					}
					break;

				default:
					throw new NotSupportedException($"NodeType {item.NodeType} is not supported in Select projection");
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

		private ColumnExpressionReplica CreateDetailAggregationExpression(ExpressionMetadata item)
		{
			// Detail aggregations use expressionType = 3 (Detail) with specific format
			// Example: "[ContactCommunication:Contact].Id" for counting ContactCommunication details
			//
			// Correct format:
			// Expression = new ColumnExpressionReplica {
			//     ExpressionType = 3,  // Detail
			//     FunctionType = 2,    // Aggregation
			//     ColumnPath = "[ContactCommunication:Contact].Id",
			//     SubFilters = ...     // For Where conditions (from case1.txt lines 91-115)
			// }

			var aggregationType = GetAggregationType(item.MethodName);
			var detailPath = item.Parameter.ColumnPath;

			// Build the column path for detail aggregation
			// Format: [DetailSchema:LinkColumn].AggregatedColumn
			var columnPath = $"{detailPath}.Id"; // Id is typically used for Count

			// Build SubFilters from DetailChain if exists
			IFilterGroup subFilters = null;
			if (item.DetailChain != null && item.DetailChain.Items.Any())
			{
				// Extract detail schema name from path: "[ContactCommunication:Contact]" -> "ContactCommunication"
				var detailSchemaName = ExtractDetailSchemaName(detailPath);

				// Build filters from the chain
				subFilters = BuildDetailSubFilters(item.DetailChain, detailSchemaName);
			}

			return new ColumnExpressionReplica
			{
				ExpressionType = (EntitySchemaQueryExpressionType)3, // Detail (not in enum, use direct value)
				ColumnPath = columnPath,
				FunctionType = FunctionType.Aggregation,
				AggregationType = aggregationType,
				SubFilters = subFilters
			};
		}

		private string ExtractDetailSchemaName(string detailPath)
		{
			// Extract schema name from "[ContactCommunication:Contact]" -> "ContactCommunication"
			if (detailPath.StartsWith("[") && detailPath.Contains(":"))
			{
				var startIndex = 1; // Skip '['
				var endIndex = detailPath.IndexOf(':');
				return detailPath.Substring(startIndex, endIndex - startIndex);
			}
			return null;
		}

		private IFilterGroup BuildDetailSubFilters(ExpressionMetadataChain detailChain, string rootSchemaName)
		{
			// Build filter group from chain items
			// Based on case1.txt structure (lines 91-115)
			var filterGroup = new FilterGroupReplica
			{
				RootSchemaName = rootSchemaName,
				LogicalOperation = LogicalOperationStrict.And,
				IsEnabled = true,
				FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType.FilterGroup,
				Items = new Dictionary<string, IFilter>()
			};

			// Process each chain item (Where filters)
			foreach (var chainItem in detailChain.Items)
			{
				if (chainItem.ExpressionMetadata != null)
				{
					// Generate filter from metadata
					var filter = ModelQueryFilterBuilder.GenerateFilter(chainItem.ExpressionMetadata);
					var filterId = Guid.NewGuid().ToString();
					filterGroup.Items.Add(filterId, filter);
				}
			}

			return filterGroup.Items.Any() ? filterGroup : null;
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
