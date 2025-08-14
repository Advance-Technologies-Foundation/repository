namespace ATF.Repository
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.Mapping;
	using ATF.Repository.Providers;
	using Terrasoft.Common;

	#region Class: AppProcessContext

	internal class AppProcessContext: IAppProcessContext
	{
		#region Fields: Private

		private readonly IDataProvider _dataProvider;

		#endregion

		#region Constructors: Public

		public AppProcessContext(IDataProvider dataProvider) {
			_dataProvider = dataProvider;
		}

		#endregion

		#region Methods: Private

		private T GetResultBusinessModel<T>(T process, List<BusinessProcessItem> items) where T : IBusinessProcess, new() {
			var resultModel = new T();
			items.ForEach(item => {
				var value = item.PropertyInfo.GetValue(process);
				item.PropertyInfo.SetValue(resultModel, value);
			});
			return resultModel;
		}

		private void ApplyResponseValuesOnProcessModel<T>(T process, List<BusinessProcessItem> processProperties, Dictionary<string, object> responseResponseValues) where T : IBusinessProcess {
			processProperties.Where(x=>(x.Direction == BusinessProcessParameterDirection.Bidirectional || x.Direction == BusinessProcessParameterDirection.Output) && responseResponseValues.ContainsKey(x.ProcessParameterName)).ToList().ForEach(
				item => {
					var value = responseResponseValues[item.ProcessParameterName];
					try {
						item.PropertyInfo.SetValue(process, value);
					} catch (Exception e) {
						throw new Exception(
							$"Cannot set value {value} into process model property {item.ProcessParameterName} for process model {process.GetType().Name}: {e.Message}");
					}
				});
		}

		private List<IExecuteProcessRequestItem> GetResultPropertyItems(List<BusinessProcessItem> processProperties) {
			return processProperties.Where(x =>
				x.Direction == BusinessProcessParameterDirection.Output ||
				x.Direction == BusinessProcessParameterDirection.Bidirectional).Select(x =>
				(IExecuteProcessRequestItem)new ExecuteProcessRequestItem() {
					Code = x.ProcessParameterName,
					DataValueType = x.DataValueType
				}).ToList();
		}

		private IBusinessProcessResponse<T> RunProcess<T>(string businessProcessName,
			List<BusinessProcessItem> inputValues) {
			var serializedInputValues = GetSerializedInputValues(inputValues);
			throw new NotImplementedException();
		}

		private Dictionary<string, string> GetSerializedInputValues(List<BusinessProcessItem> inputProperties) {
			var response = new Dictionary<string, string>();
			inputProperties.ForEach(item => {
				if (BusinessProcessValueConverter.TrySerializeProcessValue(item.DataValueType, item.Value,
					out string serializedValue)) {
					response.Add(item.ProcessParameterName, serializedValue);
				}
			});
			return response;
		}

		private List<BusinessProcessItem> GetInputProperties(List<BusinessProcessItem> items) {
			return items.Where(x =>
				x.Direction == BusinessProcessParameterDirection.Input ||
				x.Direction == BusinessProcessParameterDirection.Bidirectional).ToList();
		}

		#endregion

		#region Methods: Public

		public IBusinessProcessResponse<T> RunProcess<T>(T process) where T : IBusinessProcess, new() {
			var response = new BusinessProcessResponse<T>();
			try {
				var businessProcessName = ModelUtilities.GetBusinessProcessName(process.GetType());
				var processProperties = BusinessProcessMapper.GetParameters(process);
				var inputItems = GetInputProperties(processProperties);
				var inputParameters = GetSerializedInputValues(inputItems);
				var resultParameters = GetResultPropertyItems(processProperties);

				var providerResponse = _dataProvider.ExecuteProcess(new ExecuteProcessRequest() {
					ProcessSchemaName = businessProcessName,
					InputParameters = inputParameters,
					ResultParameters = resultParameters
				});

				response.Success = providerResponse.Success;
				response.ProcessId = providerResponse.ProcessId;
				response.ProcessStatus = providerResponse.ProcessStatus;
				response.ErrorMessage = providerResponse.ErrorMessage;
				response.Result = GetResultBusinessModel<T>(process, processProperties);
				if (providerResponse.Success && providerResponse.ResponseValues != null && providerResponse.ResponseValues.IsNotEmpty()) {
					ApplyResponseValuesOnProcessModel<T>(response.Result, processProperties, providerResponse.ResponseValues);
				}
			} catch (Exception e) {
				response.Success = false;
				response.ErrorMessage = e.Message;
			}

			return response;
		}

		#endregion

	}

	#endregion

}