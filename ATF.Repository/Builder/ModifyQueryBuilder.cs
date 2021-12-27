using System;
using System.Collections.Generic;
using System.Linq;
using ATF.Repository.Mapping;
using Terrasoft.Common;
using Terrasoft.Core.Entities;
using Terrasoft.Nui.ServiceModel.DataContract;
using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;

namespace ATF.Repository.Builder
{
	internal static class ModifyQueryBuilder
	{
		public static BaseQuery BuildModifyQuery(ITrackedModel<BaseModel> trackedModel) {
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
		private static BaseQuery BuildDeleteQuery(ITrackedModel<BaseModel> trackedModel) {
			var rootSchemaName = trackedModel.Model.GetSchemaName();
			var deleteQuery = new DeleteQuery() {
				RootSchemaName = rootSchemaName,
				IncludeProcessExecutionData = true,
				Filters = new Filters() {
					FilterType = FilterType.FilterGroup,
					Items = new Dictionary<string, Filter>() {
						{"PrimaryFilter", GeneratePrimaryFilter(trackedModel)}
					},
				}
			};
			return deleteQuery;
		}

		private static Filter GeneratePrimaryFilter(ITrackedModel<BaseModel> trackedModel) {
			return new Filter() {
				FilterType = FilterType.CompareFilter,
				LeftExpression = new ColumnExpression() {
					ExpressionType = EntitySchemaQueryExpressionType.SchemaColumn,
					ColumnPath = "Id"
				},
				ComparisonType = FilterComparisonType.Equal,
				RightExpression = new BaseExpression() {
					ExpressionType = EntitySchemaQueryExpressionType.Parameter,
					Parameter = new Parameter() {
						Value = trackedModel.Model.Id,
						DataValueType = DataValueType.Guid
					}
				}
			};
		}

		private static BaseQuery BuildUpdateQuery(ITrackedModel<BaseModel> trackedModel) {
			var rootSchemaName = trackedModel.Model.GetSchemaName();
			var values = trackedModel.GetChanges();
			if (!values.Any()) {
				return null;
			}

			var updateQuery = new UpdateQuery() {
				RootSchemaName = rootSchemaName,
				IncludeProcessExecutionData = true,
				ColumnValues = new ColumnValues() {
					Items = ConvertValuesToColumnExpressions(trackedModel.Type, values)
				},
				Filters = new Filters() {
					FilterType = FilterType.FilterGroup,
					Items = new Dictionary<string, Filter>() {
						{"PrimaryFilter", GeneratePrimaryFilter(trackedModel)}
					},
				}
			};
			return updateQuery;
		}

		private static BaseQuery BuildInsertQuery(ITrackedModel<BaseModel> trackedModel) {
			var rootSchemaName = trackedModel.Model.GetSchemaName();
			var values = trackedModel.Model.GetModelPropertyValues();
			var insertQuery = new InsertQuery() {
				RootSchemaName = rootSchemaName,
				IncludeProcessExecutionData = true,
				ColumnValues = new ColumnValues() {
					Items = ConvertValuesToColumnExpressions(trackedModel.Type, values)
				}
			};
			return insertQuery;
		}

		private static Dictionary<string, ColumnExpression> ConvertValuesToColumnExpressions(Type modelType,
			Dictionary<string, object> values) {
			var response = new Dictionary<string, ColumnExpression>();
			var properties = ModelMapper.GetProperties(modelType);
			properties.AddRange(ModelMapper.GetLookups(modelType).Where(x=>!properties.Any(y=>y.EntityColumnName == x.EntityColumnName)));
			values.Where(item=>properties.Any(x=>x.EntityColumnName == item.Key)).ForEach(item => {
				var property = properties.First(x => x.EntityColumnName == item.Key);
				if (!response.ContainsKey(property.EntityColumnName)) {
					response.Add(property.EntityColumnName, ConvertValueToColumnExpression(property, item.Value));
				}
			});
			return response;
		}

		private static ColumnExpression ConvertValueToColumnExpression(ModelItem property, object value) {
			return new ColumnExpression() {
				ExpressionType = EntitySchemaQueryExpressionType.Parameter,
				Parameter = new Parameter() {
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
