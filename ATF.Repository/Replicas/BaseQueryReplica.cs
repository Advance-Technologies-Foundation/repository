namespace ATF.Repository.Replicas
{
	using Newtonsoft.Json;
	using Terrasoft.Common;

	internal abstract class BaseQueryReplica: IBaseQuery
	{
		[JsonProperty("__type")]
		public virtual string TypeName { get; set; }

		[JsonProperty("rootSchemaName")]
		public string RootSchemaName { get; set; }

		[JsonProperty("queryKind")]
		public QueryKind QueryKind { get; set; }

		[JsonProperty("columnValues")]
		public IBaseQueryColumns ColumnValues { get; set; }

		[JsonProperty("includeProcessExecutionData")]
		public bool IncludeProcessExecutionData { get; set; }

		protected BaseQueryReplica() {
			ColumnValues = new BaseQueryColumnsReplica();
			IncludeProcessExecutionData = true;
		}
	}
}
