namespace ATF.Repository.Builder
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.Mapping;
	using ATF.Repository.Replicas;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;
	using DataValueType = Terrasoft.Nui.ServiceModel.DataContract.DataValueType;
	using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;

	internal static class ModifyQueryBuilder
	{
		public static IBaseQuery BuildModifyQuery(ITrackedModel<BaseModel> trackedModel) {
			switch (trackedModel.GetStatus()) {
				case ModelState.New:
					return BuildInsertQuery(trackedModel);
				case ModelState.Changed:
					return BuildUpdateQuery(trackedModel);
				case ModelState.Deleted:
					return BuildDeleteQuery(trackedModel);
				default:
					throw new NotSupportedException();
			}
		}
		private static BaseQueryReplica BuildDeleteQuery(ITrackedModel<BaseModel> trackedModel) {
			var rootSchemaName = trackedModel.Model.GetSchemaName();
			var deleteQuery = new DeleteQueryReplica() {
				RootSchemaName = rootSchemaName,
				IncludeProcessExecutionData = true,
				Filters = new FilterGroupReplica() {
					FilterType = FilterType.FilterGroup,
					Items = new Dictionary<string, IFilter>() {
						{"PrimaryFilter", GeneratePrimaryFilter(trackedModel)}
					},
				}
			};
			return deleteQuery;
		}

		private static FilterReplica GeneratePrimaryFilter(ITrackedModel<BaseModel> trackedModel) {
			return new FilterReplica() {
				FilterType = FilterType.CompareFilter,
				LeftExpression = new ColumnExpressionReplica() {
					ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn,
					ColumnPath = "Id"
				},
				ComparisonType = FilterComparisonType.Equal,
				RightExpression = new BaseExpressionReplica() {
					ExpressionType = EntitySchemaQueryExpressionType.Parameter,
					Parameter = new ParameterReplica() {
						Value = trackedModel.Model.Id,
						DataValueType = DataValueType.Guid
					}
				}
			};
		}

		private static BaseQueryReplica BuildUpdateQuery(ITrackedModel<BaseModel> trackedModel) {
			var rootSchemaName = trackedModel.Model.GetSchemaName();
			var values = trackedModel.GetChanges();
			if (!values.Any()) {
				return null;
			}

			var updateQuery = new UpdateQueryReplica() {
				RootSchemaName = rootSchemaName,
				ColumnValues = ConvertValuesToColumnExpressions(trackedModel.Type, values),
				Filters = new FilterGroupReplica() {
					FilterType = FilterType.FilterGroup,
					Items = new Dictionary<string, IFilter>() {
						{"PrimaryFilter", GeneratePrimaryFilter(trackedModel)}
					},
				}
			};
			return updateQuery;
		}

		private static BaseQueryReplica BuildInsertQuery(ITrackedModel<BaseModel> trackedModel) {
			var rootSchemaName = trackedModel.Model.GetSchemaName();
			var values = trackedModel.Model.GetModelPropertyValues();
			var insertQuery = new InsertQueryReplica() {
				RootSchemaName = rootSchemaName,
				ColumnValues =  ConvertValuesToColumnExpressions(trackedModel.Type, values),
			};
			return insertQuery;
		}

		private static BaseQueryColumnsReplica ConvertValuesToColumnExpressions(Type modelType,
			Dictionary<string, object> values) {
			var response = new Dictionary<string, IColumnExpression>();
			var properties = ModelMapper.GetProperties(modelType);
			properties.AddRange(ModelMapper.GetLookups(modelType).Where(x=>!properties.Any(y=>y.EntityColumnName == x.EntityColumnName)));
			values.Where(item=>properties.Any(x=>x.EntityColumnName == item.Key)).ForEach(item => {
				var property = properties.First(x => x.EntityColumnName == item.Key);
				if (!response.ContainsKey(property.EntityColumnName)) {
					response.Add(property.EntityColumnName, ConvertValueToColumnExpression(property, item.Value));
				}
			});
			return new BaseQueryColumnsReplica() { Items = response };
		}

		private static ColumnExpressionReplica ConvertValueToColumnExpression(ModelItem property, object value) {
			return new ColumnExpressionReplica() {
				ExpressionType = EntitySchemaQueryExpressionType.Parameter,
				Parameter = new ParameterReplica() {
					DataValueType = DataValueTypeUtilities.ConvertTypeToDataValueType(property.DataValueType),
					Value = ConvertValue(property.DataValueType, value)
				}
			};
		}

		private static object ConvertValue(Type valueType, object value) {
			if (valueType == typeof(Guid) && (value == null || (Guid) value == Guid.Empty)) {
				return null;
			}

			if (valueType == typeof(DateTime)) {
				if (value == null || (DateTime)value == DateTime.MinValue) {
					return null;
				}

				return $"\"{((DateTime)value):yyyy-MM-ddTHH:mm:ss.fff}\"";
			}

			return value;
		}
	}
}
