namespace ATF.Repository.Attributes
{
	using System;

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class SchemaPropertyAttribute: Attribute
	{
		public string Name { get; private set; }

		public SchemaPropertyAttribute(string name) {
			Name = name;
		}
	}
}
