namespace ATF.Repository.Replicas
{
	using Newtonsoft.Json;
	using DataValueType = Terrasoft.Nui.ServiceModel.DataContract.DataValueType;

	internal class ParameterReplica: IParameter
	{
		[JsonProperty("dataValueType")]
		public DataValueType DataValueType { get; set; }

		[JsonProperty("value")]
		public object Value { get; set; }
	}
}
