namespace ATF.Repository
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
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

		/// <summary>
		/// Converts a value from JSON deserialization to target type.
		/// Handles conversions from Newtonsoft.Json types (long, decimal, string).
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="targetType">The target type.</param>
		/// <returns>Converted value.</returns>
		private object ConvertCompositeValue(object value, System.Type targetType) {
			if (value == null) {
				return null;
			}

			// Handle numeric type conversions (JSON numbers come as long or decimal)
			if (value is long longValue) {
				if (targetType == typeof(int)) {
					return Convert.ToInt32(longValue);
				}
				if (targetType == typeof(decimal)) {
					return Convert.ToDecimal(longValue);
				}
				if (targetType == typeof(float)) {
					return Convert.ToSingle(longValue);
				}
				if (targetType == typeof(double)) {
					return Convert.ToDouble(longValue);
				}
				return longValue;
			}

			if (value is decimal decValue) {
				if (targetType == typeof(int)) {
					return Convert.ToInt32(decValue);
				}
				if (targetType == typeof(long)) {
					return Convert.ToInt64(decValue);
				}
				if (targetType == typeof(float)) {
					return Convert.ToSingle(decValue);
				}
				if (targetType == typeof(double)) {
					return Convert.ToDouble(decValue);
				}
				return decValue;
			}

			if (value is double doubleValue) {
				if (targetType == typeof(int)) {
					return Convert.ToInt32(doubleValue);
				}
				if (targetType == typeof(decimal)) {
					return Convert.ToDecimal(doubleValue);
				}
				return doubleValue;
			}

			// Handle string conversions
			if (targetType == typeof(string)) {
				return value.ToString();
			}

			// Handle Guid conversions
			if (targetType == typeof(Guid) && Guid.TryParse(value.ToString(), out var guid)) {
				return guid;
			}

			// Handle DateTime conversions
			if (targetType == typeof(DateTime) && DateTime.TryParse(value.ToString(), out var dateTime)) {
				return dateTime;
			}

			// Handle bool conversions
			if (targetType == typeof(bool) && bool.TryParse(value.ToString(), out var boolValue)) {
				return boolValue;
			}

			return value;
		}

		/// <summary>
		/// Deserializes a value to a List of custom objects with BusinessProcessParameter attributes.
		/// Handles both JSON strings (from LocalProvider) and already-deserialized objects (from RemoteProvider).
		/// </summary>
		/// <param name="propertyInfo">The property to set the value on.</param>
		/// <param name="process">The process model instance.</param>
		/// <param name="value">The value to deserialize - can be JSON string or List of objects.</param>
		/// <param name="elementType">The type of elements in the list.</param>
		private void ApplyCustomObjectListValue<T>(System.Reflection.PropertyInfo propertyInfo, T process, object value, System.Type elementType) where T : IBusinessProcess {
			if (value == null) {
				return;
			}

			List<Dictionary<string, object>> deserialized = null;

			// Handle JSON string (LocalProvider)
			if (value is string jsonValue) {
				if (string.IsNullOrEmpty(jsonValue)) {
					return;
				}
				deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonValue);
			}
			// Handle already-deserialized objects (RemoteProvider returns Newtonsoft.Json.Linq.JArray)
			else if (value is Newtonsoft.Json.Linq.JArray jArray) {
				deserialized = jArray.ToObject<List<Dictionary<string, object>>>();
			}
			// Handle already-deserialized List<Dictionary<string, object>>
			else if (value is List<Dictionary<string, object>> list) {
				deserialized = list;
			}
			// Handle already-deserialized List<elementType> (when RemoteProvider deserializes directly to target type)
			else if (value is System.Collections.IList existingList && value.GetType().IsGenericType) {
				// The value is already a List<elementType> - just set it and return
				propertyInfo.SetValue(process, value);
				return;
			}

			if (deserialized == null) {
				return;
			}

			var properties = Mapping.BusinessProcessMapper.GetBusinessProcessProperties(elementType);
			var listType = typeof(List<>).MakeGenericType(elementType);
			var resultList = System.Activator.CreateInstance(listType) as System.Collections.IList;

			foreach (var dictItem in deserialized) {
				var elementInstance = System.Activator.CreateInstance(elementType);

				foreach (var prop in properties) {
					var attr = prop.GetCustomAttribute(typeof(Attributes.BusinessProcessParameterAttribute)) as Attributes.BusinessProcessParameterAttribute;
					if (attr == null) {
						continue;
					}

					if (dictItem.TryGetValue(attr.Name, out var itemValue) && itemValue != null) {
						var convertedValue = ConvertCompositeValue(itemValue, prop.PropertyType);
						prop.SetValue(elementInstance, convertedValue);
					}
				}

				resultList?.Add(elementInstance);
			}

			propertyInfo.SetValue(process, resultList);
		}

		private void ApplyResponseValuesOnProcessModel<T>(T process, List<BusinessProcessItem> processProperties, Dictionary<string, object> responseResponseValues) where T : IBusinessProcess {
			processProperties.Where(x=>(x.Direction == BusinessProcessParameterDirection.Bidirectional || x.Direction == BusinessProcessParameterDirection.Output) && responseResponseValues.ContainsKey(x.ProcessParameterName)).ToList().ForEach(
				item => {
					var value = responseResponseValues[item.ProcessParameterName];
					try {
						// Check if this is a custom object list
						if (item.DataValueType.IsGenericType &&
						    item.DataValueType.GetGenericTypeDefinition() == typeof(List<>)) {
							var elementType = item.DataValueType.GetGenericArguments()[0];
							if (Mapping.BusinessProcessMapper.HasBusinessProcessParameters(elementType)) {
								ApplyCustomObjectListValue(item.PropertyInfo, process, value, elementType);
								return;
							}
						}

						// Default behavior for primitive types
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

		private Dictionary<string, object> GetRawInputValues(List<BusinessProcessItem> inputProperties) {
			var response = new Dictionary<string, object>();
			inputProperties.ForEach(item => {
				// Only add if it wasn't serialized (i.e. complex objects like List<CustomObject>)
				if (!BusinessProcessValueConverter.TrySerializeProcessValue(item.DataValueType, item.Value,
					out string serializedValue)) {
					response.Add(item.ProcessParameterName, item.Value);
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
				var rawInputParameters = GetRawInputValues(inputItems);
				var resultParameters = GetResultPropertyItems(processProperties);

				var providerResponse = _dataProvider.ExecuteProcess(new ExecuteProcessRequest() {
					ProcessSchemaName = businessProcessName,
					InputParameters = inputParameters,
					RawInputParameters = rawInputParameters,
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