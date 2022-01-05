namespace ATF.Repository.Replicas
{
	using Newtonsoft.Json;

	internal class BaseQueryColumnReplica: IBaseQueryColumn
	{
		[JsonProperty("expression")]
		public IColumnExpression Expression { get; set; }
	}
}
