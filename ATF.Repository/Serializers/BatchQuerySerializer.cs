namespace ATF.Repository.Serializers
{
	using System;
	using System.Linq;
	using Newtonsoft.Json;
	using ATF.Repository.Replicas;

	internal static class BatchQuerySerializer
	{
		public static string Serialize(BatchQueryReplica batchQuery) {
			var itemsPart = string.Join(",", batchQuery.Queries.Select(SerializeBatchQueryItem));
			return "{\"items\":[" + itemsPart + $"],\"includeProcessExecutionData\":true,\"instanceId\":\"{Guid.NewGuid()}\"" + "}";
		}

		private static string SerializeBatchQueryItem(IBaseQuery query) {
			var rawData = JsonConvert.SerializeObject(query);
			//var typeName = query.GetType().FullName;
			var data = rawData.Insert(1, $"\"__type\":\"{query.TypeName}\",");
			return data;
		}


	}
}
