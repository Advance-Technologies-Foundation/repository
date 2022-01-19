namespace ATF.Repository.Providers
{
	using System.Collections.Generic;

	internal class ExecuteResponse: IExecuteResponse
	{
		//public IExecuteResponseStatus ResponseStatus { get; }
		public bool Success { get; set; }

		public List<IExecuteItemResponse> QueryResults { get; set; }

		public string ErrorMessage { get; set; }
	}
}
