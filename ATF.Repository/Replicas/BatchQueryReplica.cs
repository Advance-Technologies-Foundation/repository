namespace ATF.Repository.Replicas
{
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	internal class BatchQueryReplica: IBatchQuery
	{
		[JsonProperty("items")]
		public List<IBaseQuery> Queries { get; set; }

		[JsonProperty("continueIfError")]
		public bool ContinueIfError { get; set; }

		[JsonProperty("instanceId")]
		public Guid InstanceId { get; set; }

		[JsonProperty("includeProcessExecutionData")]
		public bool IncludeProcessExecutionData { get; }

		internal BatchQueryReplica() {
			IncludeProcessExecutionData = true;
			InstanceId = Guid.NewGuid();
		}
	}
}
