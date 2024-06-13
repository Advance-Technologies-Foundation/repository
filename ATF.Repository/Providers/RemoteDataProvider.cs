using Creatio.Client;

namespace ATF.Repository.Providers
{
	using ATF.Repository.Replicas;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;
	using System.Runtime.Serialization;
	using System.Threading;
	using Castle.DynamicProxy.Internal;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Terrasoft.Common;
	using DataValueType = Terrasoft.Nui.ServiceModel.DataContract.DataValueType;

	public class RemoteDataProvider: IDataProvider
	{
		#region Fields: Private

		private readonly string _applicationUrl;

		private readonly bool _isNetCore;
		private string SelectEndpointUri => _isNetCore ? "/DataService/json/SyncReply/SelectQuery" :  "/0/DataService/json/SyncReply/SelectQuery";
		private string BatchEndpointUrl => _isNetCore ? "/DataService/json/SyncReply/BatchQuery": "/0/DataService/json/SyncReply/BatchQuery";
		private string SysSettingEndpointUrl => _isNetCore ? "/DataService/json/SyncReply/QuerySysSettings" :  "/0/DataService/json/SyncReply/QuerySysSettings";
		private string FeatureEndpointUrl => _isNetCore ? "/rest/FeatureService/GetFeatureState": "/0/rest/FeatureService/GetFeatureState";

		#endregion;

		#region Properties: Internal

		internal ICreatioClientAdapter CreatioClientAdapter;

		#endregion

		#region Constructors: Public

		public RemoteDataProvider(ICreatioClient client, string applicationUrl, bool isNetCore = false) {
			_applicationUrl = applicationUrl;
			_isNetCore = isNetCore;
			CreatioClientAdapter = new CreatioClientAdapter(client);
		}
		
		
		public RemoteDataProvider(string applicationUrl, string username, string password, bool isNetCore = false) {
			_applicationUrl = applicationUrl;
			_isNetCore = isNetCore;
			CreatioClientAdapter = new CreatioClientAdapter(applicationUrl, username, password, isNetCore);
		}

		/// <summary>
		/// Initializes a new instance of the CreatioClient class with NTLM authentication.
		/// </summary>
		/// <param name="applicationUrl">Application Url (e.g.: https://somename.creatio.com)</param>
		/// <param name="credentials">See <see cref="ICredentials"/></param>
		/// <param name="isNetCore">Optional parameter, default value <c>false</c></param>
		public RemoteDataProvider(string applicationUrl, ICredentials credentials, bool isNetCore = false) {
			_applicationUrl = applicationUrl;
			_isNetCore = isNetCore;
			CreatioClientAdapter = new CreatioClientAdapter(applicationUrl, credentials, isNetCore);
		}
		public RemoteDataProvider(string applicationUrl, string authApp, string clientId, string clientSecret, bool isNetCore = false) {
			_applicationUrl = applicationUrl;
			_isNetCore = isNetCore;
			CreatioClientAdapter = new CreatioClientAdapter(applicationUrl, authApp, clientId, clientSecret, isNetCore);
		}

		#endregion

		#region Methods: Private

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
			if (token == null || (string.IsNullOrEmpty(token.ToString()) && !IsNullableType(typeof(T)))) {
				return default(T);
			}
			return token.ToObject<T>();
		}

		private bool IsNullableType(Type type) {
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
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

		public IItemsResponse GetItems(ISelectQuery selectQuery) {
			var response = new ItemsResponse() {
				Success = false,
				Items = new List<Dictionary<string, object>>()
			};
			try {
				var requestData = JsonConvert.SerializeObject(selectQuery);
				var url = _applicationUrl + SelectEndpointUri;
				var responseBody = CreatioClientAdapter.ExecutePostRequest(url, requestData, 1800000);
				var selectResponse = JsonConvert.DeserializeObject<SelectResponse>(responseBody);
				response.Items = ParseSelectResponse(selectResponse);
				response.Success = true;
			} catch (Exception e) {
				response.ErrorMessage = e.Message + (e.InnerException != null ? e.InnerException.Message : "");
			}
			return response;
		}

		public IExecuteResponse BatchExecute(List<IBaseQuery> queries) {
			var response = new ExecuteResponse();
			var batchQuery = new BatchQueryReplica() { Queries = queries };
			var requestData = JsonConvert.SerializeObject(batchQuery); //BatchQuerySerializer.Serialize(batchQuery);
			var url = _applicationUrl + BatchEndpointUrl;
			try {
				var responseBody = CreatioClientAdapter.ExecutePostRequest(url, requestData, 600000);
				var batchResponse = JsonConvert.DeserializeObject<BatchResponse>(responseBody);
				response = ConvertBatchResponse(batchResponse);
			} catch (WebException e) {
				response.ErrorMessage = e.Message + (e.InnerException != null ? e.InnerException.Message : "");
				UpdateErrorMessageFromResponse(response, e);
			}
			return response;
		}

		private void UpdateErrorMessageFromResponse(ExecuteResponse executeResponse, WebException exception) {
			var responseStream = exception?.Response?.GetResponseStream();
			if (responseStream == null) {
				return;
			}
			try {
				var responseBody = new StreamReader(responseStream).ReadToEnd();
				var response = JsonConvert.DeserializeObject<ExecuteParsedResponse>(responseBody);
				executeResponse.ErrorMessage = response.ResponseStatus?.Message ?? executeResponse.ErrorMessage;
			} catch (Exception e) {
				executeResponse.ErrorMessage = $"{executeResponse.ErrorMessage} | {e.Message}";
			}
		}

		public T GetSysSettingValue<T>(string sysSettingCode) {
			var request = new SysSettingsRequest()
				{ SysSettingsNameCollection = new List<string>() { sysSettingCode } };
			var requestData = JsonConvert.SerializeObject(request);
			var url = _applicationUrl + SysSettingEndpointUrl;
			var responseBody = CreatioClientAdapter.ExecutePostRequest(url, requestData, 600000);
			var sysSettingRaw = JsonConvert.DeserializeObject<SysSettingsResponse>(responseBody);
			return ParseSysSettingValueResponse<T>(sysSettingRaw, sysSettingCode);
		}

		public bool GetFeatureEnabled(string featureCode) {
			var request = new FeatureRequest() { code = featureCode };
			var requestData = JsonConvert.SerializeObject(request);
			var url = _applicationUrl + FeatureEndpointUrl;
			var responseBody = CreatioClientAdapter.ExecutePostRequest(url, requestData, 600000);
			var featureRaw = JsonConvert.DeserializeObject<ServiceFeatureResponse>(responseBody);
			return featureRaw?.FeatureState == 1;
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

	internal class ServiceFeatureResponse
	{
		public int FeatureState { get; set; }
	}
}
