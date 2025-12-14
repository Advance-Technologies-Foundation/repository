namespace ATF.Repository.Mock
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.Mock.Internal;
	using ATF.Repository.Providers;
	using Terrasoft.Core.Process;

	#region Class: BaseDataProviderMock

	/// <summary>
	/// Базовий абстрактний клас для DataProvider Mock реалізацій.
	/// Містить спільну логіку для SysSettings, Features та ExecuteProcess.
	/// </summary>
	public abstract class BaseDataProviderMock : IDataProvider
	{

		#region Fields: Private

		private readonly Dictionary<string, object> _sysSettingMockValues;
		private readonly Dictionary<string, bool> _featureMockValues;
		private readonly List<ExecuteProcessMock> _executeProcessMocks;

		#endregion

		#region Constructors: Protected

		protected BaseDataProviderMock() {
			_sysSettingMockValues = new Dictionary<string, object>();
			_featureMockValues = new Dictionary<string, bool>();
			_executeProcessMocks = new List<ExecuteProcessMock>();
		}

		#endregion

		#region Methods: Public - SysSettings

		/// <summary>
		/// Налаштовує mock значення системного налаштування
		/// </summary>
		public void MockSysSettingValue<T>(string sysSettingCode, T value) {
			if (_sysSettingMockValues.ContainsKey(sysSettingCode)) {
				_sysSettingMockValues[sysSettingCode] = value;
			} else {
				_sysSettingMockValues.Add(sysSettingCode, value);
			}
		}

		/// <summary>
		/// Отримує mock значення системного налаштування
		/// </summary>
		public T GetSysSettingValue<T>(string sysSettingCode) {
			if (_sysSettingMockValues.ContainsKey(sysSettingCode) &&
				_sysSettingMockValues[sysSettingCode] is T typedValue) {
				return typedValue;
			}

			if (typeof(T) == typeof(string)) {
				return (T)Convert.ChangeType(string.Empty, typeof(T));
			}

			return default(T);
		}

		#endregion

		#region Methods: Public - Features

		/// <summary>
		/// Налаштовує mock значення фічі
		/// </summary>
		public void MockFeatureEnable(string featureCode, bool value) {
			if (_featureMockValues.ContainsKey(featureCode)) {
				_featureMockValues[featureCode] = value;
			} else {
				_featureMockValues.Add(featureCode, value);
			}
		}

		/// <summary>
		/// Отримує mock значення фічі
		/// </summary>
		public bool GetFeatureEnabled(string featureCode) {
			if (_featureMockValues.ContainsKey(featureCode)) {
				return _featureMockValues[featureCode];
			}

			return false;
		}

		#endregion

		#region Methods: Public - ExecuteProcess

		/// <summary>
		/// Налаштовує mock для виконання бізнес-процесу
		/// </summary>
		public IExecuteProcessMock MockExecuteProcess(string processSchemaName) {
			var mock = new ExecuteProcessMock(processSchemaName);
			_executeProcessMocks.Add(mock);
			return mock;
		}

		/// <summary>
		/// Виконує бізнес-процес через mock
		/// </summary>
		public IExecuteProcessResponse ExecuteProcess(IExecuteProcessRequest request) {
			var mock = _executeProcessMocks
				.Where(x => x.Enabled)
				.FirstOrDefault(x => x.CheckByRequest(request));

			if (mock == null) {
				return new Internal.ExecuteProcessResponse {
					Success = false,
					ErrorMessage = $"No mock found for process '{request.ProcessSchemaName}'. " +
						$"Available mocks: {string.Join(", ", _executeProcessMocks.Select(m => m.SchemaName))}",
					ProcessStatus = ProcessStatus.Error,
					ProcessId = Guid.Empty,
					ResponseValues = new Dictionary<string, object>()
				};
			}

			mock.OnReceived(request);
			return mock.GetResponse();
		}

		#endregion

		#region Methods: Abstract - Must be implemented by derived classes

		/// <summary>
		/// Отримує значення за замовчуванням для схеми
		/// </summary>
		public abstract IDefaultValuesResponse GetDefaultValues(string schemaName);

		/// <summary>
		/// Отримує колекцію елементів за запитом
		/// </summary>
		public abstract IItemsResponse GetItems(ISelectQuery selectQuery);

		/// <summary>
		/// Виконує пакет запитів (Insert/Update/Delete)
		/// </summary>
		public abstract IExecuteResponse BatchExecute(List<IBaseQuery> queries);

		#endregion

	}

	#endregion

}
