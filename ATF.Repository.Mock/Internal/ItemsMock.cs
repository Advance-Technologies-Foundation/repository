namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;

	internal class ItemsMock: BaseMock, IItemsMock
	{
		private List<Action<IItemsMock>> Listeners {get; }

		private void EnrichResponseItems(List<Dictionary<string, object>> items) {
			items.ForEach(item => {
				if (!item.ContainsKey("Id")) {
					item.Add("Id", Guid.NewGuid());
				}
			});
		}

		public ItemsMock(string schemaName) : base(schemaName) {
			Listeners = new List<Action<IItemsMock>>();
		}


		public IItemsMock FilterHas(object filterValue) {
			ExpectedParameters.Add(PrepareValue(filterValue));
			return this;
		}

		public IItemsMock Returns(List<Dictionary<string, object>> items) {
			EnrichResponseItems(items);
			Success = true;
			ErrorMessage = string.Empty;
			Items = items;
			return this;
		}

		public IItemsMock Returns(bool success, string errorMessage) {
			Success = success;
			ErrorMessage = errorMessage;
			Items = new List<Dictionary<string, object>>();
			return this;
		}

		public IItemsMock ReceiveHandler(Action<IItemsMock> action) {
			Listeners.Add(action);
			return this;
		}

		public override void OnReceived() {
			base.OnReceived();
			Listeners.ForEach(x=>x.Invoke(this));
		}
	}
}
