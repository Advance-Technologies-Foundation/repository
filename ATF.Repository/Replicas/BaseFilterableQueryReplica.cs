namespace ATF.Repository.Replicas
{
	using Newtonsoft.Json;
	using QueryType = Terrasoft.Nui.ServiceModel.DataContract.QueryType;

	internal abstract class BaseFilterableQueryReplica: BaseQueryReplica, IBaseFilterableQuery
	{
		[JsonProperty("queryType")]
		public abstract QueryType QueryType { get; }

		[JsonProperty("filters")]
		public IFilterGroup Filters { get; set; }
	}
}
