namespace ATF.Repository.Providers
{
	using System;

	internal class ExecuteItemResponse: IExecuteItemResponse
	{
		public Guid Id { get; set; }
		public int RowsAffected { get; set; }
		public bool Success { get; set; }

		public string ErrorMessage { get; set; }
	}

	internal class ExecuteParsedResponse: ExecuteItemResponse
	{
		public ExecuteStatus ResponseStatus { get; set; }
	}
}
