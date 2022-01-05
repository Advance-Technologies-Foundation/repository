namespace ATF.Repository.Replicas
{
	internal class InsertQueryReplica: BaseQueryReplica, IInsertQuery
	{
		public override string TypeName => "Terrasoft.Nui.ServiceModel.DataContract.InsertQuery";
	}
}
