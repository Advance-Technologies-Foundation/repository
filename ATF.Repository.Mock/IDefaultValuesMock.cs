namespace ATF.Repository.Mock
{
	using System;
	using System.Collections.Generic;

	public interface IDefaultValuesMock
	{
		string SchemaName { get; }
		int ReceivedCount { get; }
		IDefaultValuesMock Retunrs(Dictionary<string, object> defaultValues);
		IDefaultValuesMock Retunrs(bool success, string errorMessage);
		IDefaultValuesMock ReceiveHandler(Action<IDefaultValuesMock> action);
	}
}
