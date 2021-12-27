using System;
using System.Collections.Generic;
using System.Linq;

namespace ATF.Repository.Mock.Internal
{
	internal class MockSavingItem: IMockSavingItem
	{
		public string SchemaName { get; }
		public SavingOperation Operation { get; }
		public int ReceivedCount { get; private set; }
		private List<object> ExpectedParameters { get; }
		private List<object> ExpectedColumnValues { get; }
		private List<Action<IMockSavingItem>> Listeners {get;}

		public MockSavingItem(string schemaName, SavingOperation operation) {
			SchemaName = schemaName;
			Operation = operation;
			ReceivedCount = 0;
			ExpectedParameters = new List<object>();
			ExpectedColumnValues = new List<object>();
			Listeners = new List<Action<IMockSavingItem>>();
		}

		public bool CheckByParameters(List<object> parameters) {
			return ExpectedParameters.All(x=>parameters.Any(y=>Equals(x, y)));
		}

		public bool CheckByColumnValues(List<object> parameters) {
			return ExpectedColumnValues.All(x=>parameters.Any(y=>Equals(x, y)));
		}

		protected static object PrepareValue(object value) {
			return value is DateTime ? $"\"{(DateTime)value:yyyy-MM-ddTHH:mm:ss.fff}\"" : value;
		}

		public IMockSavingItem FilterHas(object filterValue) {
			ExpectedParameters.Add(PrepareValue(filterValue));
			return this;
		}

		public IMockSavingItem ChangedValueHas(object filterValue) {
			ExpectedColumnValues.Add(PrepareValue(filterValue));
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
