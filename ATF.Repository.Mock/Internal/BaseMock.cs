namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.Providers;

	internal class BaseMock
	{
		public int Position { get; set; }
		public bool Enabled { get; set; }
		public string SchemaName { get; }
		public int ReceivedCount { get; private set; }

		protected bool Success { get; set; }
		protected string ErrorMessage { get; set; }
		protected List<Dictionary<string, object>> Items { get; set; }
		internal List<object> ExpectedParameters { get; }

		protected BaseMock(string schemaName) {
			SchemaName = schemaName;
			Success = true;
			Enabled = true;
			ExpectedParameters = new List<object>();
		}

		protected static object PrepareValue(object value) {
			return value is DateTime ? $"\"{(DateTime)value:yyyy-MM-ddTHH:mm:ss.fff}\"" : value;
		}

		public bool CheckByParameters(List<object> queryParameters) {
			return ExpectedParameters.All(x=>queryParameters.Any(y=>Equals(x, y)));
		}

		public virtual void OnReceived() {
			ReceivedCount++;
		}

		public IItemsResponse GetItemsResponse() {
			return new ItemsResponse() {
				Success = Success,
				ErrorMessage = ErrorMessage,
				Items = Items
			};
		}
	}
}
