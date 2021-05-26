namespace ATF.Repository
{
	public interface ISaveResult
	{
		bool Success { get; }

		int RowsAffected { get; }

		string ErrorMessage { get; }
	}

	internal class SaveResult : ISaveResult
	{
		public bool Success { get; internal set; }
		public int RowsAffected { get; internal set; }
		public string ErrorMessage { get; internal set; }
	}
}
