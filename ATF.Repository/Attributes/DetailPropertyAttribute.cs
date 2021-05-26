namespace ATF.Repository.Attributes
{
	using System;

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DetailPropertyAttribute: Attribute
	{
		public string MasterLinkPropertyName { get; internal set; }

		public string DetailLinkPropertyName { get; private set; }

		public DetailPropertyAttribute(string detailLinkPropertyName) {
			DetailLinkPropertyName = detailLinkPropertyName;
		}

	}
}
