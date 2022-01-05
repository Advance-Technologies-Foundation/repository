namespace ATF.Repository.Replicas
{
	using Newtonsoft.Json;
	using Terrasoft.Common;
	using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;

	internal class FilterGroupReplica: FilterReplica, IFilterGroup
	{
		[JsonProperty("rootSchemaName")]
		public string RootSchemaName { get; set; }

		public FilterGroupReplica(): base() {
			FilterType = FilterType.FilterGroup;
			LogicalOperation = LogicalOperationStrict.And;
		}
	}
}
