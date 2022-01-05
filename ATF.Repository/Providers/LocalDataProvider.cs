namespace ATF.Repository.Providers
{
	using ATF.Repository.Replicas;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Terrasoft.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.DB;
	using Terrasoft.Core.Entities;
	using Terrasoft.Nui.ServiceModel.Extensions;
	using SelectQueryColumns = Terrasoft.Nui.ServiceModel.DataContract.SelectQueryColumns;
	using InsertQuery = Terrasoft.Nui.ServiceModel.DataContract.InsertQuery;
	using UpdateQuery = Terrasoft.Nui.ServiceModel.DataContract.UpdateQuery;
	using DeleteQuery = Terrasoft.Nui.ServiceModel.DataContract.DeleteQuery;
	using ColumnExpression = Terrasoft.Nui.ServiceModel.DataContract.ColumnExpression;
	using BaseFilterableQuery = Terrasoft.Nui.ServiceModel.DataContract.BaseFilterableQuery;

	public class LocalDataProvider: IDataProvider
	{

		#region Fields: Private

		private readonly UserConnection _userConnection;

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

		private Dictionary<string, object> ParseSelectResult(Entity entity, SelectQueryColumns selectQueryColumns) {
			var result = new Dictionary<string, object>();
			foreach (var selectQueryColumn in selectQueryColumns.Items) {
				var value = entity.GetColumnValue(selectQueryColumn.Key);
				result.Add(selectQueryColumn.Key, value);
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

		#endregion

		#region Methods: Public

		public IDefaultValuesResponse GetDefaultValues(string schemaName) {
			var response = new DefaultValuesResponse() {
				Success = false,
				DefaultValues = new Dictionary<string, object>(),
				ErrorMessage = string.Empty
			};
			try {
				var schema = _userConnection.EntitySchemaManager.GetInstanceByName(schemaName);
				if (schema == null) {
					return response;
				}
				var entity = schema.CreateEntity(_userConnection);
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
				var selectQuery = ReplicaToOriginConverter.ConvertSelectQuery(selectQueryReplica);
				var esq = selectQuery.BuildEsq(_userConnection);
				var entityCollection = esq.GetEntityCollection(_userConnection);
				foreach (var entity in entityCollection) {
					var entityResult = ParseSelectResult(entity, selectQuery.Columns);
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
			queries.ForEach(query => {
				switch (query) {
					case InsertQueryReplica insertQuery:
						response.QueryResults.Add(ExecuteQuery(ReplicaToOriginConverter.ConvertInsertQuery(insertQuery)));
						break;
					case UpdateQueryReplica updateQuery:
						response.QueryResults.Add(ExecuteQuery(ReplicaToOriginConverter.ConvertUpdateQuery(updateQuery)));
						break;
					case DeleteQueryReplica deleteQuery:
						response.QueryResults.Add(ExecuteQuery(ReplicaToOriginConverter.ConvertDeleteQuery(deleteQuery)));
						break;
				}
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

	}
}
