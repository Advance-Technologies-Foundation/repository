namespace ATF.Repository.Replicas
{
	using Newtonsoft.Json;
	using Terrasoft.Common;
	using QueryType = Terrasoft.Nui.ServiceModel.DataContract.QueryType;
	using QuerySource = Terrasoft.Nui.ServiceModel.DataContract.QuerySource;

	internal class SelectQueryReplica: BaseFilterableQueryReplica, ISelectQuery
	{
		public override string TypeName => "Terrasoft.Nui.ServiceModel.DataContract.SelectQuery";

		[JsonProperty("queryType")]
		public override QueryType QueryType => QueryType.Select;

		[JsonProperty("columns")]
		public ISelectQueryColumns Columns { get; set; }

		[JsonProperty("allColumns")]
		public bool AllColumns { get; set; }

		//public ServerESQCacheParameters ServerESQCacheParameters { get; set; }

		[JsonProperty("isDistinct")]
		public bool IsDistinct { get; set; }

		[JsonProperty("rowCount")]
		public int RowCount { get; set; }

		[JsonProperty("chunkSize")]
		public int ChunkSize { get; set; }

		[JsonProperty("rowsOffset")]
		public int RowsOffset { get; set; }

		[JsonProperty("isPageable")]
		public bool IsPageable { get; set; }

		[JsonProperty("useLocalization")]
		public bool UseLocalization { get; set; }

		[JsonProperty("useRecordDeactivation")]
		public bool UseRecordDeactivation { get; set; }

		[JsonProperty("queryOptimize")]
		public bool QueryOptimize { get; set; }

		[JsonProperty("useMetrics")]
		public bool UseMetrics { get; set; }

		[JsonProperty("adminUnitRoleSources")]
		public AdminUnitRoleSources AdminUnitRoleSources { get; set; }

		[JsonProperty("querySource")]
		public QuerySource QuerySource { get; set; }

		[JsonProperty("ignoreDisplayValues")]
		public bool IgnoreDisplayValues { get; set; }

		//public ColumnValues ConditionalValues { get; set; }

		//public bool IsHierarchical { get; set; }

		//public int HierarchicalMaxDepth { get; set; }

		//public string HierarchicalColumnName { get; set; }

		//public string HierarchicalColumnValue { get; set; }

		public SelectQueryReplica() {
			UseLocalization = true;
			RowCount = -1;
			RowsOffset = -1;
			ChunkSize = -1;
			AdminUnitRoleSources = AdminUnitRoleSources.None;
			QuerySource = QuerySource.Undefined;
			Columns = new SelectQueryColumnsReplica();
		}
	}
}
