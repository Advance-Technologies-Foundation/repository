namespace ATF.Repository.Providers
{
	using ATF.Repository.Replicas;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Terrasoft.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.DB;
	using Terrasoft.Core.Entities;
	using Terrasoft.Core.Process;
	using Terrasoft.Nui.ServiceModel.Extensions;
	using SelectQueryColumns = Terrasoft.Nui.ServiceModel.DataContract.SelectQueryColumns;
	using InsertQuery = Terrasoft.Nui.ServiceModel.DataContract.InsertQuery;
	using UpdateQuery = Terrasoft.Nui.ServiceModel.DataContract.UpdateQuery;
	using DeleteQuery = Terrasoft.Nui.ServiceModel.DataContract.DeleteQuery;
	using ColumnExpression = Terrasoft.Nui.ServiceModel.DataContract.ColumnExpression;
	using BaseFilterableQuery = Terrasoft.Nui.ServiceModel.DataContract.BaseFilterableQuery;

	public class LocalDataProvider: IDataProvider
	{
		internal class BuiltEntitySchemaQuery
		{
			public EntitySchemaQuery EntitySchemaQuery { get; set; }
			public EntitySchemaQueryOptions Options { get; set; }
			public Dictionary<string, string> ServerToClientColumnNameMap { get; set; }
		}

		#region Fields: Private

		private readonly UserConnection _userConnection;

		#endregion

		#region Fields: Public

		public bool EnableAccessRights = false;

		#endregion

		#region Constructors: Public

		public LocalDataProvider(UserConnection userConnection) {
			_userConnection = userConnection;
		}

		#endregion

		#region Methods: Private

		private bool TryGetEntity(string schemaName, out Entity entity, out string errorMessage) {
			entity = null;
			errorMessage = string.Empty;
			var schema = _userConnection.EntitySchemaManager.GetInstanceByName(schemaName);
			if (schema == null) {
				errorMessage = $"Cannot find entity schema with name {schemaName}";
				return false;
			}
			entity = schema.CreateEntity(_userConnection);
			entity.UseAdminRights = EnableAccessRights;
			return true;
		}

		private bool TryGetEntity(string schemaName, Guid primaryValue, out Entity entity, out string errorMessage) {
			entity = null;
			errorMessage = string.Empty;
			if (!TryGetEntity(schemaName, out entity, out errorMessage)) {
				return false;
			}
			if (!entity.FetchFromDB(primaryValue)) {
				errorMessage = $"Cannot find entity ({schemaName}) by primary value {primaryValue}";
				return false;
			}
			return true;
		}

		private IExecuteItemResponse ExecuteQuery(InsertQuery insertQuery) {
			var response =  new ExecuteItemResponse() {
				Success = false,
				RowsAffected = 0,
				ErrorMessage = string.Empty
			};

			if (!TryGetEntity(insertQuery.RootSchemaName, out var entity, out var errorMessage)) {
				response.ErrorMessage = errorMessage;
				return response;
			}
			entity.SetDefColumnValues();
			SetColumnValues(entity, insertQuery.ColumnValues.Items);
			try {
				response.Success = entity.Save(false);
				response.RowsAffected = 1;
			} catch (Exception e) {
				response.ErrorMessage = e.Message;
			}
			return response;
		}

		private static void SetColumnValues(Entity entity, Dictionary<string, ColumnExpression> columnValuesItems) {
			columnValuesItems.ForEach(item => {
				var column = entity.Schema.Columns.FindByName(item.Key);
				if (column != null) {
					entity.SetColumnValue(column.ColumnValueName, item.Value.Parameter.Value);
				}
			});
		}

		private Dictionary<string, object> ParseSelectResult(Entity entity, ISelectQueryColumns selectQueryColumns,
			Dictionary<string, string> serverToClientColumnNameMap) {
			var result = new Dictionary<string, object>();
			var schema = entity.Schema;
			foreach (var selectQueryColumn in selectQueryColumns.Items) {
				var serverToClientKeyValuePair =
					serverToClientColumnNameMap?.FirstOrDefault(x => x.Value == selectQueryColumn.Key) ?? null;
				var key = serverToClientKeyValuePair != null
					? serverToClientKeyValuePair.Value.Key
					: selectQueryColumn.Key;
				var column = schema.Columns.FindByName(key);
				if (column != null) {
					var value = entity.GetColumnValue(column.ColumnValueName);
					result.Add(selectQueryColumn.Key, value);
				}
			}

			return result.Any() ? result : null;
		}

		private IExecuteItemResponse ExecuteQuery(UpdateQuery updateQuery) {
			var response =  new ExecuteItemResponse() {
				Success = false,
				RowsAffected = 0,
				ErrorMessage = string.Empty
			};
			if (!TryParsePrimaryValue(updateQuery, out var primaryValue)) {
				response.ErrorMessage = "Cannot find primary value from filter";
				return response;
			}
			if (!TryGetEntity(updateQuery.RootSchemaName, primaryValue, out var entity, out var errorMessage)) {
				response.ErrorMessage = errorMessage;
				return response;
			}
			SetColumnValues(entity, updateQuery.ColumnValues.Items);
			try {
				response.Success = entity.Save(false);
				response.RowsAffected = 1;
			} catch (Exception e) {
				response.ErrorMessage = e.Message;
			}
			return response;
		}


		private IExecuteItemResponse ExecuteQuery(DeleteQuery deleteQuery) {
			var response =  new ExecuteItemResponse() {
				Success = false,
				RowsAffected = 0,
				ErrorMessage = string.Empty
			};
			if (!TryParsePrimaryValue(deleteQuery, out var primaryValue)) {
				response.ErrorMessage = "Cannot find primary value from filter";
				return response;
			}
			if (!TryGetEntity(deleteQuery.RootSchemaName, primaryValue, out var entity, out var errorMessage)) {
				response.ErrorMessage = errorMessage;
				return response;
			}
			try {
				response.Success = entity.Delete();
				response.RowsAffected = 1;
			} catch (Exception e) {
				response.ErrorMessage = e.Message;
			}
			return response;
		}

		private static bool TryParsePrimaryValue(BaseFilterableQuery updateQuery, out Guid output) {
			output = Guid.Empty;
			var filterItem = updateQuery?.Filters?.Items?.FirstOrDefault(x => x.Key == "PrimaryFilter");
			if (filterItem?.Value == null) {
				return false;
			}
			var primaryFilter = filterItem?.Value;
			if (primaryFilter.LeftExpression?.ColumnPath != "Id") {
				return false;
			}
			if (!(primaryFilter?.RightExpression?.Parameter?.Value is Guid guidValue)) {
				return false;
			}
			output = guidValue;
			return true;
		}

		private BuiltEntitySchemaQuery BuildEntitySchemaQuery(ISelectQuery source) {
			var selectQuery = ReplicaToOriginConverter.ConvertSelectQuery(source);
			var esq = selectQuery.BuildEsq(_userConnection, out var serverToClientColumnNameMap);
			esq.HideSecurityValue = false;
			esq.RowCount = source.RowCount;
			var options = source.RowsOffset > 0
				? QueryExtension.GetEntitySchemaQueryOptions(selectQuery, null, _userConnection)
				: null;
			return new BuiltEntitySchemaQuery() {
				EntitySchemaQuery = esq,
				Options = options,
				ServerToClientColumnNameMap = serverToClientColumnNameMap
			};
		}

		private void WrapInTransactionIf(bool useTransaction, Action action) {
			if (useTransaction) {
				using (var dbExecutor = _userConnection.EnsureDBConnection()) {
					dbExecutor.StartTransaction();
					action();
					dbExecutor.CommitTransaction();
				}
			} else {
				action();
			}
		}

		private int GetFeatureState(string code) {
			var select = (Select)new Select(_userConnection).Top(1)
				.Column("AdminUnitFeatureState", "FeatureState")
				.From("AdminUnitFeatureState")
				.InnerJoin("Feature").On("Feature", "Id").IsEqual("AdminUnitFeatureState", "FeatureId")
				.InnerJoin("SysAdminUnitInRole").On("SysAdminUnitInRole", "SysAdminUnitRoleId")
				.IsEqual("AdminUnitFeatureState", "SysAdminUnitId")
				.Where("Feature", "Code").IsEqual(Column.Parameter(code))
				.And("SysAdminUnitInRole", "SysAdminUnitId").IsEqual(Column.Parameter(_userConnection.CurrentUser.Id))
				.OrderByDesc("AdminUnitFeatureState", "FeatureState");
			return select.ExecuteScalar<int>();
		}

		#endregion

		#region Methods: Public

		public IDefaultValuesResponse GetDefaultValues(string schemaName) {
			var response = new DefaultValuesResponse() {
				Success = false,
				DefaultValues = new Dictionary<string, object>(),
				ErrorMessage = string.Empty
			};
			try {
				if (!TryGetEntity(schemaName, out var entity, out var errorMessage)) {
					response.ErrorMessage = errorMessage;
					return response;
				}
				entity.SetDefColumnValues();
				foreach (var entitySchemaColumn in entity.Schema.Columns) {
					if (entitySchemaColumn.HasDefValue && entitySchemaColumn.Name != "Id") {
						response.DefaultValues.Add(entitySchemaColumn.Name,
							entity.GetColumnValue(entitySchemaColumn.ColumnValueName));
					}
				}
				response.Success = true;
			} catch (Exception e) {
				response.ErrorMessage = e.Message;
			}
			return response;
		}

		public IItemsResponse GetItems(ISelectQuery selectQueryReplica) {
			var response = new ItemsResponse() {
				Success = false,
				Items = new List<Dictionary<string, object>>(),
				ErrorMessage = string.Empty
			};
			try {
				var buildData = BuildEntitySchemaQuery(selectQueryReplica);
				buildData.EntitySchemaQuery.UseAdminRights = EnableAccessRights;
				var entityCollection = buildData.Options != null
					? buildData.EntitySchemaQuery.GetEntityCollection(_userConnection, buildData.Options)
					: buildData.EntitySchemaQuery.GetEntityCollection(_userConnection);
				foreach (var entity in entityCollection) {
					var entityResult = ParseSelectResult(entity, selectQueryReplica.Columns, buildData.ServerToClientColumnNameMap);
					if (entityResult != null) {
						response.Items.Add(entityResult);
					}
				}
				response.Success = true;
			} catch (Exception e) {
				response.ErrorMessage = e.Message;
			}

			return response;
		}

		public IExecuteResponse BatchExecute(List<IBaseQuery> queries) {
			var response = new ExecuteResponse() { QueryResults = new List<IExecuteItemResponse>() };
			WrapInTransactionIf(true, () => {
				queries.ForEach(query => {
					switch (query) {
						case InsertQueryReplica insertQueryReplica:
							var insertQuery = ReplicaToOriginConverter.ConvertInsertQuery(insertQueryReplica);
							response.QueryResults.Add(ExecuteQuery(insertQuery));
							break;
						case UpdateQueryReplica updateQueryReplica:
							var updateQuery = ReplicaToOriginConverter.ConvertUpdateQuery(updateQueryReplica);
							response.QueryResults.Add(ExecuteQuery(updateQuery));
							break;
						case DeleteQueryReplica deleteQueryReplica:
							var deleteQuery = ReplicaToOriginConverter.ConvertDeleteQuery(deleteQueryReplica);
							response.QueryResults.Add(ExecuteQuery(deleteQuery));
							break;
					}
				});
			});
			response.Success = response.QueryResults.All(x => x.Success);
			return response;
		}


		public T GetSysSettingValue<T>(string sysSettingCode) {
			return Terrasoft.Core.Configuration.SysSettings.GetValue(_userConnection, sysSettingCode,
				default(T));
		}

		public bool GetFeatureEnabled(string featureCode) {
			return GetFeatureState(featureCode) == 1;
		}

		private string SerializeRawParameter(object value) {
			if (value == null) {
				return string.Empty;
			}

			if (value is System.Collections.IEnumerable enumerable && !(value is string)) {
				var compositeList = new CompositeObjectList<CompositeObject>();
				foreach (var item in enumerable) {
					if (item == null) continue;

					var compositeObject = new CompositeObject();
					var properties = ATF.Repository.Mapping.BusinessProcessMapper.GetBusinessProcessProperties(item.GetType());

					foreach (var prop in properties) {
						var attr = prop.GetCustomAttribute(typeof(ATF.Repository.Attributes.BusinessProcessParameterAttribute))
							as ATF.Repository.Attributes.BusinessProcessParameterAttribute;
						if (attr == null) continue;

						var propValue = prop.GetValue(item);
						if (propValue != null) {
							// Convert to string for CompositeObject
							compositeObject[attr.Name] = propValue.ToString();
						}
					}

					compositeList.Add(compositeObject);
				}

				return BaseSerializableObjectUtilities.SerializeToJson(compositeList);
			}

			return value.ToString();
		}

		public IExecuteProcessResponse ExecuteProcess(IExecuteProcessRequest request) {
			var response = new ExecuteProcessResponse() {
				ResponseValues = new Dictionary<string, object>(),
			};
			try {
				var processEngine = _userConnection.ProcessEngine;
				var processExecutor = processEngine.ProcessExecutor;
				var resultParameterNames = request.ResultParameters?.Select(x => x.Code).ToList() ?? new List<string>();

				var allParameters = new Dictionary<string, string>(request.InputParameters ?? new Dictionary<string, string>());

				if (request.RawInputParameters != null) {
					foreach (var rawParam in request.RawInputParameters) {
						var serialized = SerializeRawParameter(rawParam.Value);
						allParameters[rawParam.Key] = serialized;
					}
				}

				var processDescriptor = processExecutor.Execute(request.ProcessSchemaName, allParameters,
					resultParameterNames);
				response.ProcessStatus = processDescriptor.ProcessStatus;
				response.ProcessId = processDescriptor.UId;
				if (processDescriptor.ProcessStatus == ProcessStatus.Done) {
					var resultParameterValues = processDescriptor.ResultParameterValues;
					if (resultParameterValues != null) {
						resultParameterNames.ForEach(resultParameterName => {
							if (resultParameterValues.TryGetValue(resultParameterName, out var value)) {
								response.ResponseValues.Add(resultParameterName, value);
							}
						});
					}
				}
				response.Success = response.ProcessStatus == ProcessStatus.Error;
			} catch (Exception e) {
				response.ErrorMessage = e.Message;
				response.ProcessStatus = ProcessStatus.Error;
			}
			return response;
		}

		#endregion

	}
}
