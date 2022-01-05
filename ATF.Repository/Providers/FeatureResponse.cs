namespace ATF.Repository.Providers
{
	internal class FeatureResponse: IFeatureResponse
	{
		public bool Success { get; internal set; }
		public bool Enabled { get; internal set; }
		public string ErrorMessage { get; internal set; }
	}
}
