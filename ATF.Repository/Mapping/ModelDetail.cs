namespace ATF.Repository.Mapping
{
	using System;

	internal class ModelDetail: LazyMapInfo
	{
		public string MasterFilterPropertyName { get; set; }

		public string DetailFilterPropertyName { get; set; }

	}
}
