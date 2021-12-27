namespace ATF.Repository.Mock
{
	using System;

	public interface IMockSavingItem
	{
		string SchemaName { get; }
		SavingOperation Operation { get; }
		int ReceivedCount { get; }
		IMockSavingItem FilterHas(object filterValue);
		IMockSavingItem ChangedValueHas(object filterValue);
		IMockSavingItem ReceiveHandler(Action<IMockSavingItem> action);
	}
}
