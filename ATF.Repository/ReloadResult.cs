namespace ATF.Repository
{
	using ATF.Repository.Providers;

	internal class ReloadResult : IReloadResult
	{
		public bool Success { get; set; }
		public string ErrorMessage { get; set; }
	}
}
