namespace ATF.Repository.Replicas
{
	using Newtonsoft.Json;
	using QueryType = Terrasoft.Nui.ServiceModel.DataContract.QueryType;

	internal class DeleteQueryReplica : BaseFilterableQueryReplica, IDeleteQuery
	{
		public override string TypeName => "Terrasoft.Nui.ServiceModel.DataContract.DeleteQuery";

		[JsonProperty("queryType")]
		public override QueryType QueryType => QueryType.Delete;
	}
}
