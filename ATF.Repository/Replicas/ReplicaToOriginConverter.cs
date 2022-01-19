namespace ATF.Repository.Replicas
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Terrasoft.Common;
	using Terrasoft.Nui.ServiceModel.DataContract;

	internal static class ReplicaToOriginConverter
	{
		private static string _dateTimePattern = @"(\d{4}).(\d{2}).(\d{2})T(\d{2}):(\d{2}):(\d{2})";
		public static  SelectQuery ConvertSelectQuery(ISelectQuery source) {
			return new SelectQuery() {
				RootSchemaName = source.RootSchemaName,
				AdminUnitRoleSources = source.AdminUnitRoleSources,
				AllColumns = source.AllColumns,
				ChunkSize = source.ChunkSize,
				Columns = ConvertSelectQueryColumnsReplicaToSelectQueryColumns(source.Columns),
				ColumnValues = ConvertBaseQueryColumnsReplicaToColumnValues(source.ColumnValues),
				Filters = ConvertFilterGroupReplicaToFilters(source.Filters),
				IsDistinct = source.IsDistinct,
				RowCount = source.RowCount,
				RowsOffset = source.RowsOffset,
				IsPageable = source.IsPageable,
				UseLocalization = source.UseLocalization,
				UseRecordDeactivation = source.UseRecordDeactivation,
				QueryOptimize = source.QueryOptimize,
				UseMetrics = source.UseMetrics,
				QuerySource = source.QuerySource,
				IgnoreDisplayValues = source.IgnoreDisplayValues
			};
		}

		public static InsertQuery ConvertInsertQuery(IInsertQuery source) {
			var response = new InsertQuery();
			EnrichBaseQueryByBaseQueryReplica(response, source);
			return response;
		}

		public static UpdateQuery ConvertUpdateQuery(IUpdateQuery source) {
			var response = new UpdateQuery();
			EnrichBaseQueryByBaseQueryReplica(response, source);
			EnrichBaseFilterableQueryByBaseFilterableQueryReplica(response, source);
			response.IsForceUpdate = source.IsForceUpdate;
			return response;
		}

		public static DeleteQuery ConvertDeleteQuery(IDeleteQuery source) {
			var response = new DeleteQuery();
			EnrichBaseQueryByBaseQueryReplica(response, source);
			EnrichBaseFilterableQueryByBaseFilterableQueryReplica(response, source);
			return response;
		}

		private static void EnrichBaseFilterableQueryByBaseFilterableQueryReplica(BaseFilterableQuery target,
			IBaseFilterableQuery source) {
			target.Filters = ConvertFilterGroupReplicaToFilters(source.Filters);
		}

		private static void EnrichBaseQueryByBaseQueryReplica(BaseQuery target, IBaseQuery source) {
			target.RootSchemaName = source.RootSchemaName;
			target.QueryKind = source.QueryKind;
			target.ColumnValues = ConvertBaseQueryColumnsReplicaToColumnValues(source.ColumnValues);
		}

		private static SelectQueryColumns ConvertSelectQueryColumnsReplicaToSelectQueryColumns(ISelectQueryColumns source) {
			var response = new SelectQueryColumns() { Items = new Dictionary<string, SelectQueryColumn>() };
			source?.Items.ForEach(x => {
				response.Items.Add(x.Key, ConvertSelectQueryColumnReplicaToSelectQueryColumn(x.Value));
			});
			return response;
		}

		private static SelectQueryColumn ConvertSelectQueryColumnReplicaToSelectQueryColumn(ISelectQueryColumn source) {
			return new SelectQueryColumn() {
				OrderDirection = source.OrderDirection,
				OrderPosition = source.OrderPosition,
				Expression = ConvertColumnExpressionReplicaToColumnExpression(source.Expression)
			};
		}

		private static ColumnExpression ConvertColumnExpressionReplicaToColumnExpression(IColumnExpression sourceExpression) {
			var response = new ColumnExpression();
			EnrichBaseExpressionFromBaseExpressionReplica(response, sourceExpression);
			return response;
		}

		private static void EnrichBaseExpressionFromBaseExpressionReplica(BaseExpression target, IBaseExpression source) {
			target.Parameter = ConvertParameterReplicaToParameter(source.Parameter);
			target.AggregationType = source.AggregationType;
			target.ArithmeticOperation = source.ArithmeticOperation;
			target.ColumnPath = source.ColumnPath;
			target.ExpressionType = source.ExpressionType;
			target.FunctionArgument = source.FunctionArgument != null ? ConvertBaseExpressionReplicaToBaseExpression(source.FunctionArgument) : null;
			target.FunctionArguments = source.FunctionArguments?.Select(ConvertBaseExpressionReplicaToBaseExpression)
				.ToArray();
			target.FunctionType = source.FunctionType;
			target.SubFilters = ConvertFilterGroupReplicaToFilters(source.SubFilters);
			target.LeftArithmeticOperand = source.LeftArithmeticOperand != null ? ConvertBaseExpressionReplicaToBaseExpression(source.LeftArithmeticOperand) : null;
			target.RightArithmeticOperand = source.RightArithmeticOperand != null ? ConvertBaseExpressionReplicaToBaseExpression(source.RightArithmeticOperand) : null;
		}

		private static BaseExpression ConvertBaseExpressionReplicaToBaseExpression(IBaseExpression sourceExpression) {
			var response = new BaseExpression();
			EnrichBaseExpressionFromBaseExpressionReplica(response, sourceExpression);
			return response;
		}

		private static Parameter ConvertParameterReplicaToParameter(IParameter sourceParameter) {
			// ToDo: to save
			if (sourceParameter == null) {
				return null;
			}
			return new Parameter() {
				DataValueType = sourceParameter.DataValueType,
				Value = sourceParameter.Value
			};
		}

		private static object ConvertParameterValue(Parameter source) {
			if (DataValueTypeUtilities.IsDateDataValueType(source.DataValueType)) {
				return ParseDateTimeParameterValue(source.Value);
			}
			return source.Value;
		}

		private static DateTime ParseDateTimeParameterValue(object source) {
			if (source is string stringValue) {
				var regex = new Regex(_dateTimePattern);
				var match = regex.Match(stringValue);
				return new DateTime(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value),
					int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value),
					int.Parse(match.Groups[5].Value), int.Parse(match.Groups[6].Value));
			} else {
				return default;
			}
		}

		private static ColumnValues ConvertBaseQueryColumnsReplicaToColumnValues(IBaseQueryColumns source) {
			var response = new ColumnValues() { Items = new Dictionary<string, ColumnExpression>() };
			if (source?.Items == null) {
				return response;
			}
			source.Items.ForEach(x => {
				var columnExpression = ConvertColumnExpressionReplicaToColumnExpression(x.Value);
				if (columnExpression?.Parameter != null) {
					columnExpression.Parameter.Value = ConvertParameterValue(columnExpression.Parameter);
				}
				response.Items.Add(x.Key, columnExpression);
			});
			return response;
		}

		private static Filters ConvertFilterGroupReplicaToFilters(IFilterGroup source) {
			if (source == null) {
				return null;
			}
			var response = new Filters() {Items = new Dictionary<string, Filter>()};
			EnrichFilterFromSource(response, source);
			response.RootSchemaName = source.RootSchemaName;
			return response;
		}

		private static Filter ConvertFilterReplicaToFilter(IFilter source) {
			if (source == null) {
				return null;
			}
			var response = new Filter() {Items = new Dictionary<string, Filter>()};
			EnrichFilterFromSource(response, source);
			return response;
		}

		private static void EnrichFilterFromSource(Filter target, IFilter source) {
			if (source == null || target == null) {
				return;
			}
			if (source.Items?.Count() > 0) {
				source.Items.ForEach(x => {
					target.Items.Add(x.Key, ConvertFilterReplicaToFilter(x.Value));
				});
			}

			target.LogicalOperation = source.LogicalOperation;
			target.IsEnabled = source.IsEnabled;
			target.FilterType = source.FilterType;
			target.ComparisonType = source.ComparisonType;
			target.IsNull = source.IsNull;
			target.IsNot = source.IsNot;
			target.SubFilters = source.SubFilters != null ? ConvertFilterGroupReplicaToFilters(source.SubFilters) : null;
			target.LeftExpression = source.LeftExpression != null
				? ConvertBaseExpressionReplicaToBaseExpression(source.LeftExpression)
				: null;
			target.RightExpression = source.RightExpression != null
				? ConvertBaseExpressionReplicaToBaseExpression(source.RightExpression)
				: null;

			if (source.RightExpressions?.Count() > 0) {
				target.RightExpressions = source.RightExpressions.Select(ConvertBaseExpressionReplicaToBaseExpression)
					.ToArray();
			}
			target.TrimDateTimeParameterToDate = source.TrimDateTimeParameterToDate;
		}

	}
}
