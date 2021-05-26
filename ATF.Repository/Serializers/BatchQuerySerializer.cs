namespace ATF.Repository.Serializers
{
	using System;
	using System.Linq;
	using Newtonsoft.Json;
	using Terrasoft.Nui.ServiceModel.DataContract;

	internal static class BatchQuerySerializer
	{
		public static string Serialize(BatchQuery batchQuery) {
			var itemsPart = string.Join(",", batchQuery.Queries.Select(SerializeBatchQueryItem));
			return "{\"items\":[" + itemsPart + $"],\"includeProcessExecutionData\":true,\"instanceId\":\"{Guid.NewGuid()}\"" + "}";
		}

		private static string SerializeBatchQueryItem(BaseQuery query) {
			var rawData = JsonConvert.SerializeObject(query);
			var typeName = query.GetType().FullName;
			var data = rawData.Insert(1, $"\"__type\":\"{typeName}\",");
			return data;
		}
	}
}
