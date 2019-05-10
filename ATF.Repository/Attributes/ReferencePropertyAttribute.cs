namespace ATF.Repository.Attributes
{
	using System;

	public class ReferencePropertyAttribute : Attribute
	{
		public string Name { get; private set; }

		public ReferencePropertyAttribute(string name) {
			Name = name;
		}
	}
}
