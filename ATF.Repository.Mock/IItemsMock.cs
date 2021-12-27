namespace ATF.Repository.Mock
{
	using System;
	using System.Collections.Generic;

	public interface IMock
	{
		string SchemaName { get; }
		int ReceivedCount { get; }
	}
	public interface IScalarMock: IMock
	{

		IScalarMock FilterHas(object filterValue);
		IScalarMock Retunrs(object value);
		IScalarMock Retunrs(bool success, string errorMessage);
		IScalarMock ReceiveHandler(Action<IScalarMock> action);
	}
	public interface IItemsMock: IMock
	{
		IItemsMock FilterHas(object filterValue);
		IItemsMock Retunrs(List<Dictionary<string, object>> items);
		IItemsMock Retunrs(bool success, string errorMessage);
		IItemsMock ReceiveHandler(Action<IItemsMock> action);
	}
}
