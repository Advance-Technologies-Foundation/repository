namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	internal class MockSavingItem: IMockSavingItem
	{
		public string SchemaName { get; }
		public SavingOperation Operation { get; }
		public int ReceivedCount { get; private set; }
		private List<object> ExpectedParameters { get; }
		private List<ExpectedColumnValueItem> ExpectedColumnValues { get; }
		private List<Action<IMockSavingItem>> Listeners {get;}

		public MockSavingItem(string schemaName, SavingOperation operation) {
			SchemaName = schemaName;
			Operation = operation;
			ReceivedCount = 0;
			ExpectedParameters = new List<object>();
			ExpectedColumnValues = new List<ExpectedColumnValueItem>();
			Listeners = new List<Action<IMockSavingItem>>();
		}

		public bool CheckByParameters(List<object> parameters) {
			return ExpectedParameters.All(x=>parameters.Any(y=>Equals(x, y)));
		}

		public bool CheckByColumnValues(List<ExpectedColumnValueItem> parameters) {
			return ExpectedColumnValues.All(x => parameters.Any(y =>
				(string.IsNullOrEmpty(x.Name) || Equals(x.Name, y.Name)) && Equals(x.Value, y.Value)));
		}

		protected static object PrepareValue(object value) {
			return value is DateTime ? $"\"{(DateTime)value:yyyy-MM-ddTHH:mm:ss.fff}\"" : value;
		}

		public IMockSavingItem FilterHas(object filterValue) {
			ExpectedParameters.Add(PrepareValue(filterValue));
			return this;
		}

		public IMockSavingItem ChangedValueHas(object value) {
			ExpectedColumnValues.Add(new ExpectedColumnValueItem() {
				Value = PrepareValue(value)
			});
			return this;
		}

		public IMockSavingItem ChangedValueHas(string schemaItemName, object value) {
			ExpectedColumnValues.Add(new ExpectedColumnValueItem() {
				Name = schemaItemName,
				Value = PrepareValue(value)
			});
			return this;
		}

		public IMockSavingItem ReceiveHandler(Action<IMockSavingItem> action) {
			Listeners.Add(action);
			return this;
		}

		public void OnReceived() {
			ReceivedCount++;
			Listeners.ForEach(x=>x.Invoke(this));
		}
	}
}
