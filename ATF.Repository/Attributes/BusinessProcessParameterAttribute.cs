namespace ATF.Repository.Attributes
{
	using System;

	public class BusinessProcessParameterAttribute: Attribute
	{
		public string Name { get; private set; }
		public BusinessProcessParameterDirection Direction { get; set; }

		public BusinessProcessParameterAttribute(string name, BusinessProcessParameterDirection direction) {
			if(string.IsNullOrWhiteSpace(name)) {
				throw new ArgumentException("BusinessProcess name cannot be empty", nameof(name));
			}
			Name = name;
			Direction = direction;
		}
	}
}