namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;
	using Terrasoft.Common;
	using Terrasoft.Core.Process;

	internal class ExecutingProcessMock: IExecutingProcessMock
	{
		internal readonly Dictionary<string, string> Inputs = new Dictionary<string, string>();
		internal readonly Dictionary<string, object> ResponseValues = new Dictionary<string, object>();
		internal readonly List<Action<IExecutingProcessMock>> Handlers = new List<Action<IExecutingProcessMock>>();

		internal DateTime CreatedOn { get; set; }
		internal Guid ProcessId { get; set; } = Guid.NewGuid();

		internal ProcessStatus ProcessStatus { get; set; } = ProcessStatus.Done;

		public string SchemaName { get; internal set; }
		public int ReceivedCount { get; internal set; }
		public bool Success { get; internal set; } = true;
		public string ErrorMessage { get; internal set; } = "";

		public bool Enabled { get; set; } = true;
		public IExecutingProcessMock HasInputParameters(Dictionary<string, string> parameters) {
			parameters.ForEach(parameter => Inputs[parameter.Key] = parameter.Value);
			return this;
		}

		public IExecutingProcessMock SetProcessStatus(ProcessStatus processStatus) {
			ProcessStatus = processStatus;
			return this;
		}

		public IExecutingProcessMock Returns(Dictionary<string, object> parameters) {
			parameters.ForEach(parameter => ResponseValues[parameter.Key] = parameter.Value);
			return this;
		}

		public IExecutingProcessMock Returns(string errorMessage) {
			Success = false;
			ErrorMessage = errorMessage;
			return this;
		}

		public IExecutingProcessMock ReceiveHandler(Action<IExecutingProcessMock> action) {
			Handlers.Add(action);
			return this;
		}

		public void OnReceived() {
			ReceivedCount++;
			Handlers.ForEach(handler => handler.Invoke(this));
		}
	}
}
