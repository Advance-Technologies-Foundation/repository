namespace ATF.Repository.Replicas
{
	using System.Collections.Generic;
	using Newtonsoft.Json;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;
	using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;

	internal class FilterReplica: IFilter
	{
		[JsonProperty("logicalOperation")]
		public LogicalOperationStrict LogicalOperation { get; set; }

		[JsonProperty("isEnabled")]
		public bool IsEnabled { get; set; }

		[JsonProperty("filterType")]
		public FilterType FilterType { get; set; }

		[JsonProperty("comparisonType")]
		public FilterComparisonType ComparisonType { get; set; }

		[JsonProperty("isNull")]
		public bool IsNull { get; set; }

		[JsonProperty("isNot")]
		public bool IsNot { get; set; }

		[JsonProperty("subFilters")]
		public IFilterGroup SubFilters { get; set; }

		[JsonProperty("items")]
		public Dictionary<string, IFilter> Items { get; set; }

		[JsonProperty("leftExpression")]
		public IBaseExpression LeftExpression { get; set; }

		[JsonProperty("rightExpression")]
		public IBaseExpression RightExpression { get; set; }

		[JsonProperty("rightExpressions")]
		public IBaseExpression[] RightExpressions { get; set; }

		[JsonProperty("trimDateTimeParameterToDate")]
		public bool TrimDateTimeParameterToDate { get; set; }

		public FilterReplica() {
			Items = new Dictionary<string, IFilter>();
			IsEnabled = true;
		}

	}
}
