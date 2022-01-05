namespace ATF.Repository.Replicas
{
	using System.Collections.Generic;
	using Newtonsoft.Json;

	internal class BaseQueryColumnsReplica: IBaseQueryColumns
	{
		[JsonProperty("items")]
		public Dictionary<string, IColumnExpression> Items { get; set; }

		public BaseQueryColumnsReplica() {
			Items = new Dictionary<string, IColumnExpression>();
		}
	}
}
