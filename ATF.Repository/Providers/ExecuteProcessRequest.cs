namespace ATF.Repository.Providers
{
	using System.Collections.Generic;

	internal class ExecuteProcessRequest : IExecuteProcessRequest
	{
		public string ProcessSchemaName { get; set; }
		public Dictionary<string, string> InputParameters { get; set; }
		public Dictionary<string, object> RawInputParameters { get; set; }
		public List<IExecuteProcessRequestItem> ResultParameters { get; set; }
	}
}