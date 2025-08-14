namespace ATF.Repository.Providers
{
	using System;

	internal class ExecuteProcessRequestItem : IExecuteProcessRequestItem
	{
		public string Code { get; set; }
		public Type DataValueType { get; set; }
	}
}