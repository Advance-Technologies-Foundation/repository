namespace ATF.Repository.Mock
{
	using System;
	using System.Collections.Generic;

	public interface IDefaultValuesMock
	{
		string SchemaName { get; }
		int ReceivedCount { get; }
		IDefaultValuesMock Returns(Dictionary<string, object> defaultValues);
		IDefaultValuesMock Returns(bool success, string errorMessage);
		IDefaultValuesMock ReceiveHandler(Action<IDefaultValuesMock> action);
	}
}
