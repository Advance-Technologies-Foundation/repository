namespace ATF.Repository.Providers
{
	using ATF.Repository.Serializers;
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Runtime.Serialization;
	using System.Threading;
	using Castle.DynamicProxy.Internal;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Terrasoft.Common;
	using Terrasoft.Nui.ServiceModel.DataContract;

	public class RemoteDataProvider: IDataProvider
	{
		#region Fields: Private

		private ICreatioClientAdapter _creatioClientAdapter;
		private readonly string _applicationUrl;
		private readonly string _username;
		private readonly string _password;
		private const string SelectEndpointUri = "/0/DataService/json/SyncReply/SelectQuery";
		private const string BatchEndpointUrl = "/0/DataService/json/SyncReply/BatchQuery";
		private const string SysSettingEndpointUrl = "/0/DataService/json/SyncReply/QuerySysSettings";
		private const string FeatureEndpointUrl = "/0/rest/FeatureService/GetFeatureState";

		#endregion;

		#region Properties: Internal

		internal ICreatioClientAdapter CreatioClientAdapter {
			get => _creatioClientAdapter ?? (_creatioClientAdapter = CreateCreatioClientAdapter());
			set => _creatioClientAdapter = value;
		}

		#endregion

		#region Constructors: Public

		public RemoteDataProvider(string applicationUrl, string username, string password, bool isNetCore = false) {
			_applicationUrl = applicationUrl;
			_username = username;
			_password = password;
		}

		#endregion

		#region Methods: Private

		private CreatioClientAdapter CreateCreatioClientAdapter() {
			return new CreatioClientAdapter(_applicationUrl, _username, _password);
		}

		private List<Dictionary<string, object>> ParseSelectResponse(SelectResponse selectResponse) {
			var response = new List<Dictionary<string, object>>();
			selectResponse.Rows.ForEach(row => {
				response.Add(ParseSelectResponseRow(selectResponse.RowConfig, row));
			});
			return response;
		}
		private Dictionary<string, object> ParseSelectResponseRow(Dictionary<string, RowConfigItem> rowConfig, JObject row) {
			var response = new Dictionary<string, object>();
			rowConfig.ForEach(rowConfigItem => {
				var dataValueType = rowConfigItem.Value.DataValueType;
				var type = DataValueTypeUtilities.ConvertDataValueTypeToType(dataValueType);
				if (!row.ContainsKey(rowConfigItem.Key) || type == null)
					return;
				var rawValue = GetValueToken(row, rowConfigItem.Key, dataValueType);
				var method =
					RepositoryReflectionUtilities.GetGenericMethod(GetType(), "ConvertJTokenToValue", type);
				var value = method?.Invoke(this, new object[] {rawValue});
				response.Add(rowConfigItem.Key, value);
			});
			return response;
		}

		private JToken GetValueToken(JObject row, string key, DataValueType dataValueType) {
			return IsLookupDataValueType(dataValueType) && row[key].IsNotEmpty() && ((JObject) row[key]).ContainsKey("value")
				? row[key]["value"]
				: row[key];
		}

		private T ConvertJTokenToValue<T>(JToken token) {
			if (token == null || (string.IsNullOrEmpty(token.ToString()) && !typeof(T).IsNullableType())) {
				return default(T);
			}
			return token.ToObject<T>();
		}

		private bool IsLookupDataValueType(DataValueType dataValueType) {
			return dataValueType == DataValueType.Lookup || dataValueType == DataValueType.Enum;
		}

		public IDefaultValuesResponse GetDefaultValues(string schemaName) {
			return new DefaultValuesResponse() {
				Success = true,
				DefaultValues = new Dictionary<string, object>()
			};
			throw new NotImplementedException();
		}

		private ExecuteResponse ConvertBatchResponse(BatchResponse batchResponse) {
			return new ExecuteResponse() {
				Success = !batchResponse.HasErrors
			};
		}

		#endregion

		#region Methods: Public

		public IItemsResponse GetItems(SelectQuery selectQuery) {
			var response = new ItemsResponse() {
				Success = false,
				Items = new List<Dictionary<string, object>>()
			};
			try {
				var requestData = JsonConvert.SerializeObject(selectQuery);
				var url = _applicationUrl + SelectEndpointUri;
				var responseBody = CreatioClientAdapter.ExecutePostRequest(url, requestData, Timeout.Infinite);
				var selectResponse = JsonConvert.DeserializeObject<SelectResponse>(responseBody);
				response.Items = ParseSelectResponse(selectResponse);
				response.Success = true;
			} catch (Exception e) {
				response.ErrorMessage = e.Message + (e.InnerException != null ? e.InnerException.Message : "");
			}
			return response;
		}

		public IExecuteResponse BatchExecute(List<BaseQuery> queries) {
			var response = new ExecuteResponse();
			var batchQuery = new BatchQuery() { Queries = queries };
			var requestData = BatchQuerySerializer.Serialize(batchQuery);
			var url = _applicationUrl + BatchEndpointUrl;
			try {
				var responseBody = CreatioClientAdapter.ExecutePostRequest(url, requestData, Timeout.Infinite);
				var batchResponse = JsonConvert.DeserializeObject<BatchResponse>(responseBody);
				response = ConvertBatchResponse(batchResponse);
			} catch (WebException e) {
				response.ErrorMessage = e.Message + (e.InnerException != null ? e.InnerException.Message : "");
			}
			return response;
		}

		public T GetSysSettingValue<T>(string sysSettingCode) {
			var request = new SysSettingsRequest()
				{ SysSettingsNameCollection = new List<string>() { sysSettingCode } };
			try {
				var requestData = JsonConvert.SerializeObject(request);
				var url = _applicationUrl + SysSettingEndpointUrl;
				var responseBody = CreatioClientAdapter.ExecutePostRequest(url, requestData, Timeout.Infinite);
				var response = JsonConvert.DeserializeObject<SysSettingsResponse>(responseBody);
				return ParseSysSettingValueResponse<T>(response, sysSettingCode);
			} catch (Exception e) {
				return default(T);
			}
		}

		public bool GetFeatureEnabled(string featureCode) {
			var request = new FeatureRequest() { code = featureCode };
			try {
				var requestData = JsonConvert.SerializeObject(request);
				var url = _applicationUrl + FeatureEndpointUrl;
				var responseBody = CreatioClientAdapter.ExecutePostRequest(url, requestData, Timeout.Infinite);
				var response = JsonConvert.DeserializeObject<FeatureResponse>(responseBody);
				return response?.FeatureState == 1;
			} catch (Exception e) {
				return false;
			}
		}

		private T ParseSysSettingValueResponse<T>(SysSettingsResponse response, string code) {
			if (response == null || !response.Success || !response.Values.ContainsKey(code)) {
				return default(T);
			}
			var responseItem = response.Values[code];
			var rawValue = responseItem?.Value;
			if (rawValue == null) {
				return default(T);
			}
			if (rawValue is T typedValue) {
				return typedValue;
			}
			if ((rawValue is long && typeof(T) == typeof(int)) || (rawValue is double && typeof(T) == typeof(decimal))) {
				return (T)Convert.ChangeType(rawValue, typeof(T));
			}

			if (rawValue is string stringValue && typeof(T) == typeof(Guid)) {
				return (T)Convert.ChangeType(new Guid(stringValue), typeof(T));
			}

			return default(T);
		}

		#endregion

	}

	internal class RowConfigItem
	{
		[DataMember(Name = "dataValueType")]
		public DataValueType DataValueType;
	}

	internal class SelectResponse
	{
		[DataMember(Name = "rowConfig")]
		public Dictionary<string, RowConfigItem> RowConfig { get; set; }

		[DataMember(Name = "rowsAffected")]
		public int RowsAffected { get; set; }

		[DataMember(Name = "success")]
		public bool Success { get; set; }

		[DataMember(Name = "rows")]
		public List<JObject> Rows { get; set; }
	}

	internal class BatchResponse
	{
		public ResponseStatus ResponseStatus { get; set; }

		public List<object> QueryResults { get; set; }

		public bool HasErrors { get; set; }
	}

	internal class ResponseStatus
	{
		public ResponseStatus(string errorCode) => this.ErrorCode = errorCode;

		public ResponseStatus(string errorCode, string message)
			: this(errorCode) {
			this.Message = message;
		}

		[JsonProperty(PropertyName = "ErrorCode")]
		public string ErrorCode { get; set; }

		[JsonProperty(PropertyName = "Message")]
		public string Message { get; set; }

		[JsonProperty(PropertyName = "StackTrace")]
		public string StackTrace { get; set; }

		[JsonProperty(PropertyName = "Errors")]
		public List<ResponseError> Errors { get; set; }

	}

	internal class ResponseError
	{
		public string ErrorCode { get; set; }

		public string FieldName { get; set; }

		public string Message { get; set; }

		public Dictionary<string, string> Meta { get; set; }
	}

	internal class SysSettingsRequest
	{
		public List<string> SysSettingsNameCollection { get; set; }
	}

	internal class SysSettingsResponse
	{
		public int RowsAffected { get; set; }
		public bool Success { get; set; }
		public Dictionary<string, SysSettingsItemResponse> Values { get; set; }
	}

	internal class SysSettingsItemResponse
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Code { get; set; }
		public object Value { get; set; }
		public DataValueType DataValueType { get; set; }
		public string TypeName { get; set; }
	}

	internal class FeatureRequest
	{
		public string code { get; set; }
	}

	internal class FeatureResponse
	{
		public int FeatureState { get; set; }
	}
}
