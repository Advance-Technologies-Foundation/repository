﻿namespace ATF.Repository.Providers
{
	using System.Collections.Generic;

	internal class ItemsResponse: IItemsResponse
	{
		public bool Success { get; set; }

		public List<Dictionary<string, object>> Items { get; set; }

		public string ErrorMessage { get; set; }
	}
}
