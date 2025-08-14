namespace ATF.Repository.Providers
{
	using System;
	using System.Collections.Generic;
	using Terrasoft.Core.Process;

	internal class ExecuteProcessResponse: IExecuteProcessResponse
	{
		public bool Success { get; internal set; }
		public string ErrorMessage { get; internal set; }
		public Guid ProcessId { get; internal set; }
		public ProcessStatus ProcessStatus { get; internal set; }

		public Dictionary<string, object> ResponseValues { get; internal set; }
	}
}