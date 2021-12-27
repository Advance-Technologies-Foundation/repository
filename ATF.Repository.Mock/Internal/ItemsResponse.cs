namespace ATF.Repository.Mock.Internal
{
	using System.Collections.Generic;
	using ATF.Repository.Providers;

	internal class ItemsResponse: IItemsResponse
	{
		public bool Success { get; set; }
		public List<Dictionary<string, object>> Items { get; set; }
		public string ErrorMessage { get; set; }
	}
}
