namespace ATF.Repository.Mock
{
	using System;
    using System.Collections.Generic;
    using System.Linq;
    using ATF.Repository.Mock.Internal;
    using ATF.Repository.Providers;
    using Terrasoft.Common;

	#region Class: BaseDataProviderMock

	public abstract class BaseDataProviderMock: IDataProvider
	{
		#region Fields: Private

		private readonly Dictionary<string, object> _sysSettingMockValues = new Dictionary<string, object>();
		private readonly Dictionary<string, bool> _featureMockValues = new Dictionary<string, bool>();
		private readonly List<ExecutingProcessMock> _executingProcessMocks = new List<ExecutingProcessMock>();

		#endregion

		#region Methods: Private

		private bool AppropriateInputs(Dictionary<string, string> mockInputs,
			Dictionary<string, string> requestInputs) {
			return requestInputs.IsEmpty()
				? mockInputs.IsEmpty()
				: mockInputs.All(x => requestInputs.ContainsKey(x.Key) && requestInputs[x.Key] == x.Value);
		}

		#endregion

		#region Methods: Public

		public abstract IDefaultValuesResponse GetDefaultValues(string schemaName);

		public abstract IItemsResponse GetItems(ISelectQuery selectQuery);

		public abstract IExecuteResponse BatchExecute(List<IBaseQuery> queries);

		public void MockSysSettingValue<T>(string sysSettingCode, T value) {
			_sysSettingMockValues[sysSettingCode] = value;
		}
		public T GetSysSettingValue<T>(string sysSettingCode) {
			if (_sysSettingMockValues.ContainsKey(sysSettingCode) && _sysSettingMockValues[sysSettingCode] is T typedValue) {
				return typedValue;
			}

			if (typeof(T) == typeof(string)) {
				return (T)Convert.ChangeType(string.Empty, typeof(T));
			}

			return default(T);
		}

		public void MockFeatureEnable(string featureCode, bool value) {
			_featureMockValues[featureCode] = value;
		}
		public bool GetFeatureEnabled(string featureCode) {
			if (_featureMockValues.TryGetValue(featureCode, out var enabled)) {
				return enabled;
			}

			return false;
		}

		public IExecutingProcessMock MockExecuteProcess(string processSchemaName) {
			var mock = new ExecutingProcessMock() {
				SchemaName = processSchemaName,
				Enabled = true,
				CreatedOn = DateTime.UtcNow
			};
			_executingProcessMocks.Add(mock);
			return mock;
		}

		public IExecuteProcessResponse ExecuteProcess(IExecuteProcessRequest request) {
			var mock = _executingProcessMocks.OrderBy(x=>x.CreatedOn).FirstOrDefault(x =>
				x.Enabled && x.SchemaName == request.ProcessSchemaName &&
				AppropriateInputs(x.Inputs, request.InputParameters));
			if (mock != null) {
				mock.OnReceived();
				return new ExecuteProcessResponse() {
					Success = mock.Success,
					ErrorMessage = mock.ErrorMessage,
					ProcessId = mock.ProcessId,
					ProcessStatus = mock.ProcessStatus,
					ResponseValues = mock.ResponseValues
				};
			} else {
				return new ExecuteProcessResponse() {
					Success = false,
					ErrorMessage = "No appropriate mock was found"
				};
			}

		}

		#endregion

	}

	#endregion

}
