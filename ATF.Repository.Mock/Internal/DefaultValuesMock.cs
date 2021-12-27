namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;
	using ATF.Repository.Providers;

	internal class DefaultValuesMock : IDefaultValuesMock
	{
		public string SchemaName { get; }
		public int ReceivedCount { get; private set; }

		private Dictionary<string, object> DefaultValues { get; set; }
		private bool Success { get; set; }
		private string ErrorMessage { get; set; }

		private List<Action<IDefaultValuesMock>> Listeners {get;}

		internal DefaultValuesMock(string schemaName) {
			SchemaName = schemaName;
			ReceivedCount = 0;
			Success = true;
			ErrorMessage = string.Empty;
			DefaultValues = new Dictionary<string, object>();
			Listeners = new List<Action<IDefaultValuesMock>>();
		}

		public IDefaultValuesResponse GetDefaultValues() {
			return new DefaultValuesResponse() {
				Success = Success,
				DefaultValues = DefaultValues,
				ErrorMessage = ErrorMessage
			};
		}

		public IDefaultValuesMock Retunrs(Dictionary<string, object> defaultValues) {
			DefaultValues = defaultValues;
			return this;
		}

		public IDefaultValuesMock Retunrs(bool success, string errorMessage) {
			Success = success;
			ErrorMessage = errorMessage;
			return this;
		}

		public IDefaultValuesMock ReceiveHandler(Action<IDefaultValuesMock> action) {
			Listeners.Add(action);
			return this;
		}

		public void OnReceived() {
			ReceivedCount++;
			Listeners.ForEach(x=>x.Invoke(this));
		}

	}
}
