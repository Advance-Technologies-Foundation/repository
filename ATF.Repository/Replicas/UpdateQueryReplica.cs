namespace ATF.Repository.Replicas
{
	using Newtonsoft.Json;
	using QueryType = Terrasoft.Nui.ServiceModel.DataContract.QueryType;

	internal class UpdateQueryReplica : BaseFilterableQueryReplica, IUpdateQuery
	{
		public override string TypeName => "Terrasoft.Nui.ServiceModel.DataContract.UpdateQuery";

		[JsonProperty("queryType")]
		public override QueryType QueryType => QueryType.Update;

		[JsonProperty("isForceUpdate")]
		public bool IsForceUpdate { get; set; }
	}
}
