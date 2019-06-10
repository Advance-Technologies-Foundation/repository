namespace ATF.Repository.Attributes
{
	using System;

	[Obsolete("Will be removed in 1.3.0")]
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class ReferencePropertyAttribute : Attribute
	{
		public string Name { get; private set; }

		public ReferencePropertyAttribute(string name) {
			Name = name;
		}
	}
}
