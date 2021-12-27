namespace ATF.Repository.Mock.Internal
{
	using System.Collections.Generic;
	using ATF.Repository.Providers;

	internal class ExecuteResponse: IExecuteResponse
	{
		public bool Success { get; set; }

		public List<IExecuteItemResponse> QueryResults { get; set; }

		public string ErrorMessage { get; set; }
	}
}
