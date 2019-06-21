namespace ATF.Repository.Attributes
{
	using System;

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class LookupPropertyAttribute: Attribute
	{
		public string Name { get; private set; }

		public LookupPropertyAttribute(string name) {
			Name = name;
		}
	}
}
