namespace ATF.Repository.Mock
{
	using System;

	public interface IMockSavingItem
	{
		string SchemaName { get; }
		SavingOperation Operation { get; }
		int ReceivedCount { get; }
		IMockSavingItem FilterHas(object filterValue);

		IMockSavingItem ChangedValueHas(object value);

		IMockSavingItem ChangedValueHas(string schemaItemName, object value);
		IMockSavingItem ReceiveHandler(Action<IMockSavingItem> action);
	}
}
