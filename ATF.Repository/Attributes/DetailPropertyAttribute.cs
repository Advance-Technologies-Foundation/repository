namespace ATF.Repository.Attributes
{
	using System;
	public class DetailPropertyAttribute: Attribute
	{
		public string MasterFilterPropertyName { get; private set; }

		public string DetailFilterPropertyName { get; private set; }

		public DetailPropertyAttribute(string detailFilterPropertyName) {
			DetailFilterPropertyName = detailFilterPropertyName;
		}

	}
}
