namespace ATF.Repository.Mock.Internal
{
	using System.Collections.Generic;
	using ATF.Repository.Providers;

	internal class DefaultValuesResponse : IDefaultValuesResponse
	{
		public bool Success { get; set; }
		public Dictionary<string, object> DefaultValues { get; set;}
		public string ErrorMessage { get; set;}
	}
}
