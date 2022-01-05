namespace ATF.Repository.Replicas
{
	using System.Collections.Generic;
	using Newtonsoft.Json;

	internal class SelectQueryColumnsReplica: ISelectQueryColumns
	{
		[JsonProperty("items")]
		public Dictionary<string, ISelectQueryColumn> Items { get; set; }

		public SelectQueryColumnsReplica() {
			Items = new Dictionary<string, ISelectQueryColumn>();
		}
	}
}
