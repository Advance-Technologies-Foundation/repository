namespace ATF.Repository.Providers
{
	internal class ExecuteItemResponse: IExecuteItemResponse
	{
		public int RowsAffected { get; set; }
		public bool Success { get; set; }

		public string ErrorMessage { get; set; }
	}

	internal class ExecuteParsedResponse: ExecuteItemResponse
	{
		public ExecuteStatus ResponseStatus { get; set; }
	}
}
