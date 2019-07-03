namespace ATF.Repository.Attributes
{
	using System;

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DetailPropertyAttribute: Attribute
	{
		public string MasterEntityColumnName { get; private set; }

		public string DetailEntityColumnName { get; private set; }

		public DetailPropertyAttribute(string detailEntityColumnName) {
			DetailEntityColumnName = detailEntityColumnName;
		}

	}
}
