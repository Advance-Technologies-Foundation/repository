namespace ATF.Repository.Attributes
{
	using System;

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class BusinessProcessAttribute: Attribute
	{
		public string Name { get; private set; }

		public BusinessProcessAttribute(string name) {
			if(string.IsNullOrWhiteSpace(name)) {
				throw new ArgumentException("BusinessProcess name cannot be empty", nameof(name));
			}
			Name = name;
		}
	}
}