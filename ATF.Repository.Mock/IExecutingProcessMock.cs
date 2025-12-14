namespace ATF.Repository.Mock
{
	using System;
	using System.Collections.Generic;
	using Terrasoft.Core.Process;

	public interface IExecutingProcessMock
	{
		string SchemaName { get; }
		int ReceivedCount { get; }
		bool Enabled { get; set; }

		IExecutingProcessMock HasInputParameters(Dictionary<string, string> parameters);
		IExecutingProcessMock SetProcessStatus(ProcessStatus processStatus);
		IExecutingProcessMock Returns(Dictionary<string, object> parameters);
		IExecutingProcessMock Returns(string errorMessage);
		IExecutingProcessMock ReceiveHandler(Action<IExecutingProcessMock> action);
	}
}
