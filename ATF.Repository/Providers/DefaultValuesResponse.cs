namespace ATF.Repository.Providers
{
	using System.Collections.Generic;

	internal class DefaultValuesResponse : IDefaultValuesResponse
	{
		public bool Success { get; set; }
		public Dictionary<string, object> DefaultValues { get; set;}
		public string ErrorMessage { get; set;}
	}
}
