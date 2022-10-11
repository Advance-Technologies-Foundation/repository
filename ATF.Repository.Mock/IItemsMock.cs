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
		IScalarMock Returns(object value);
		IScalarMock Returns(bool success, string errorMessage);
		IScalarMock ReceiveHandler(Action<IScalarMock> action);
		bool Enabled { get; set; }
	}
	public interface IItemsMock: IMock
	{
		IItemsMock FilterHas(object filterValue);
		IItemsMock Returns(List<Dictionary<string, object>> items);
		IItemsMock Returns(bool success, string errorMessage);
		IItemsMock ReceiveHandler(Action<IItemsMock> action);
		bool Enabled { get; set; }
	}
}
