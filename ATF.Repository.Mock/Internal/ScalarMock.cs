namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;

	internal class ScalarMock: BaseMock, IScalarMock
	{
		private static readonly Dictionary<AggregationScalarType, string> Alias =
			new Dictionary<AggregationScalarType, string>() {
				{AggregationScalarType.Avg, "AVERAGEValue"},
				{AggregationScalarType.Count, "COUNTValue"},
				{AggregationScalarType.Max, "MAXValue"},
				{AggregationScalarType.Min, "MINValue"},
				{AggregationScalarType.Sum, "SUMValue"},
				{AggregationScalarType.Any, "ANYValue"}
			};

		private List<Action<IScalarMock>> Listeners {get; }
		private AggregationScalarType AggregationType { get; set; }
		public ScalarMock(string schemaName, AggregationScalarType aggregationType): base(schemaName) {
			AggregationType = aggregationType;
			Listeners = new List<Action<IScalarMock>>();
		}
		public IScalarMock FilterHas(object parameterValue) {
			ExpectedParameters.Add(PrepareValue(parameterValue));
			return this;
		}

		public IScalarMock Retunrs(object value) {
			Success = true;
			ErrorMessage = string.Empty;
			Items = GetItemsForValue(value);
			return this;
		}

		public IScalarMock Retunrs(bool success, string errorMessage) {
			Success = success;
			ErrorMessage = errorMessage;
			Items = new List<Dictionary<string, object>>();
			return this;
		}

		public IScalarMock ReceiveHandler(Action<IScalarMock> action) {
			Listeners.Add(action);
			return this;
		}
		public override void OnReceived() {
			base.OnReceived();
			Listeners.ForEach(x=>x.Invoke(this));
		}

		private List<Dictionary<string, object>> GetItemsForValue(object value) {
			var columnAlias = GetAggregationColumnAlias();
			return new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{columnAlias, value}
				}
			};
		}

		private string GetAggregationColumnAlias() {
			if (Alias.ContainsKey(AggregationType)) {
				return Alias[AggregationType];
			}

			throw new NotSupportedException();
		}

	}
}
