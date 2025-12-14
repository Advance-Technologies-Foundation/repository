namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.Mapping;
	using ATF.Repository.Providers;
	using Terrasoft.Core.Process;

	internal class ExecuteProcessMock : IExecuteProcessMock
	{
		#region Fields: Private

		private bool _success;
		private string _errorMessage;
		private Dictionary<string, object> _outputParameters;
		private List<ExpectedParameterItem> _expectedInputParameters;
		private Action<IExecuteProcessMock> _receiveHandler;

		// Для перевірки фактичних отриманих параметрів
		private Dictionary<string, string> _receivedInputParameters;
		private Dictionary<string, object> _receivedRawInputParameters;

		#endregion

		#region Properties: Public

		public string SchemaName { get; }
		public int ReceivedCount { get; private set; }
		public bool Enabled { get; set; }

		#endregion

		#region Constructors: Internal

		internal ExecuteProcessMock(string processSchemaName)
		{
			SchemaName = processSchemaName;
			Enabled = true;
			_success = true;
			_errorMessage = null;
			_outputParameters = new Dictionary<string, object>();
			_expectedInputParameters = new List<ExpectedParameterItem>();
		}

		#endregion

		#region Methods: Public - Configuration

		public IExecuteProcessMock HasInputParameter(string parameterName, object expectedValue)
		{
			_expectedInputParameters.Add(new ExpectedParameterItem
			{
				Name = parameterName,
				Value = expectedValue,
				IsComplex = false
			});
			return this;
		}

		public IExecuteProcessMock HasInputParameter<T>(string parameterName, List<T> expectedValue) where T : class
		{
			_expectedInputParameters.Add(new ExpectedParameterItem
			{
				Name = parameterName,
				Value = expectedValue,
				IsComplex = true,
				ElementType = typeof(T)
			});
			return this;
		}

		public IExecuteProcessMock Returns(Dictionary<string, object> outputParameters)
		{
			_success = true;
			_outputParameters = outputParameters ?? new Dictionary<string, object>();
			return this;
		}

		public IExecuteProcessMock Returns<T>(string parameterName, List<T> outputValue) where T : class
		{
			_success = true;
			if (_outputParameters == null)
			{
				_outputParameters = new Dictionary<string, object>();
			}
			_outputParameters[parameterName] = outputValue;
			return this;
		}

		public IExecuteProcessMock Returns(bool success, string errorMessage)
		{
			_success = success;
			_errorMessage = errorMessage;
			return this;
		}

		public IExecuteProcessMock ReceiveHandler(Action<IExecuteProcessMock> action)
		{
			_receiveHandler = action;
			return this;
		}

		#endregion

		#region Methods: Public - Inspection

		public Dictionary<string, string> GetReceivedInputParameters()
		{
			return _receivedInputParameters ?? new Dictionary<string, string>();
		}

		public Dictionary<string, object> GetReceivedRawInputParameters()
		{
			return _receivedRawInputParameters ?? new Dictionary<string, object>();
		}

		#endregion

		#region Methods: Internal - Matching & Execution

		/// <summary>
		/// Перевіряє чи запит відповідає очікуванням цього mock
		/// </summary>
		internal bool CheckByRequest(IExecuteProcessRequest request)
		{
			// 1. Перевірка ProcessSchemaName
			if (request.ProcessSchemaName != SchemaName)
			{
				return false;
			}

			// 2. Якщо немає очікуваних параметрів - будь-який запит підходить
			if (_expectedInputParameters.Count == 0)
			{
				return true;
			}

			// 3. Перевірка кожного очікуваного параметру
			foreach (var expectedParam in _expectedInputParameters)
			{
				if (expectedParam.IsComplex)
				{
					// Складний параметр - шукаємо в RawInputParameters
					if (request.RawInputParameters == null ||
					    !request.RawInputParameters.ContainsKey(expectedParam.Name))
					{
						return false;
					}

					var actualValue = request.RawInputParameters[expectedParam.Name];
					if (!CompareComplexParameters(expectedParam.Value, actualValue, expectedParam.ElementType))
					{
						return false;
					}
				}
				else
				{
					// Простий параметр - шукаємо в InputParameters
					if (request.InputParameters == null ||
					    !request.InputParameters.ContainsKey(expectedParam.Name))
					{
						return false;
					}

					var actualValue = request.InputParameters[expectedParam.Name];
					if (!CompareSimpleParameters(expectedParam.Value, actualValue))
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Викликається коли запит прийнято
		/// </summary>
		internal void OnReceived(IExecuteProcessRequest request)
		{
			ReceivedCount++;

			// Зберігаємо отримані параметри для assertion
			_receivedInputParameters = new Dictionary<string, string>(
				request.InputParameters ?? new Dictionary<string, string>());
			_receivedRawInputParameters = new Dictionary<string, object>(
				request.RawInputParameters ?? new Dictionary<string, object>());

			_receiveHandler?.Invoke(this);
		}

		/// <summary>
		/// Генерує відповідь
		/// </summary>
		internal IExecuteProcessResponse GetResponse()
		{
			return new ExecuteProcessResponse
			{
				Success = _success,
				ErrorMessage = _errorMessage,
				ProcessStatus = _success ? ProcessStatus.Done : ProcessStatus.Error,
				ProcessId = Guid.NewGuid(),
				ResponseValues = _outputParameters ?? new Dictionary<string, object>()
			};
		}

		#endregion

		#region Methods: Private - Comparison

		/// <summary>
		/// Порівнює простий параметр (серіалізований)
		/// </summary>
		private bool CompareSimpleParameters(object expected, string actualSerialized)
		{
			// Серіалізуємо expected так само як BusinessProcessValueConverter
			if (!BusinessProcessValueConverter.TrySerializeProcessValue(
				    expected?.GetType() ?? typeof(object), expected, out var expectedSerialized))
			{
				// Якщо не вдалося серіалізувати - вважаємо що не збігається
				return false;
			}

			return expectedSerialized == actualSerialized;
		}

		/// <summary>
		/// Порівнює складний параметр (List&lt;CustomObject&gt;)
		/// </summary>
		private bool CompareComplexParameters(object expected, object actual, Type elementType)
		{
			// Обидва повинні бути IList
			if (!(expected is System.Collections.IList expectedList) ||
			    !(actual is System.Collections.IList actualList))
			{
				return false;
			}

			// Кількість елементів повинна збігатися
			if (expectedList.Count != actualList.Count)
			{
				return false;
			}

			// Отримуємо властивості з BusinessProcessParameter
			var properties = BusinessProcessMapper.GetBusinessProcessProperties(elementType);

			// Порівнюємо кожен елемент
			for (int i = 0; i < expectedList.Count; i++)
			{
				var expectedItem = expectedList[i];
				var actualItem = actualList[i];

				// Порівнюємо кожну властивість
				foreach (var prop in properties)
				{
					var expectedPropValue = prop.GetValue(expectedItem);
					var actualPropValue = prop.GetValue(actualItem);

					if (!Equals(expectedPropValue, actualPropValue))
					{
						return false;
					}
				}
			}

			return true;
		}

		#endregion
	}

	/// <summary>
	/// Опис очікуваного параметру
	/// </summary>
	internal class ExpectedParameterItem
	{
		public string Name { get; set; }
		public object Value { get; set; }
		public bool IsComplex { get; set; }
		public Type ElementType { get; set; }
	}
}
