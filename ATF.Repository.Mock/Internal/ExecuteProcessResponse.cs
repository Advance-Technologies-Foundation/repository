namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;
	using ATF.Repository.Providers;
	using Terrasoft.Core.Process;

	internal class ExecuteProcessResponse : IExecuteProcessResponse
	{
		public bool Success { get; set; }
		public string ErrorMessage { get; set; }
		public Guid ProcessId { get; set; }
		public ProcessStatus ProcessStatus { get; set; }
		public Dictionary<string, object> ResponseValues { get; set; }
	}
}
