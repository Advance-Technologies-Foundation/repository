namespace ATF.Repository.Replicas
{
	using Newtonsoft.Json;
	using Terrasoft.Common;

	internal class SelectQueryColumnReplica: BaseQueryColumnReplica, ISelectQueryColumn
	{
		[JsonProperty("orderDirection")]
		public OrderDirection OrderDirection { get; set; }

		[JsonProperty("orderPosition")]
		public int OrderPosition { get; set; }
	}
}
