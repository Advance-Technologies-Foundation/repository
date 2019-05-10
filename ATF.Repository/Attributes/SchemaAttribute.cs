namespace ATF.Repository.Attributes
{
	using System;

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class SchemaAttribute: Attribute
	{
		public string Name { get; private set; }

		public SchemaAttribute(string name) {
			Name = name;
		}

	}
}
