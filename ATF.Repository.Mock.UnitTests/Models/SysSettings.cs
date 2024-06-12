namespace ATF.Repository.Mock.UnitTests.Models
{
	using System;
	using ATF.Repository.Attributes;

	[Schema("SysSettings")]
	public class SysSettings: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }

		[SchemaProperty("Description")]
		public string Description { get; set; }

		[SchemaProperty("Code")]
		public string Code { get; set; }

		[SchemaProperty("ValueTypeName")]
		public string ValueTypeName { get; set; }

		[SchemaProperty("ReferenceSchemaUId")]
		public Guid ReferenceSchemaUId { get; set; }

		[SchemaProperty("IsPersonal")]
		public bool IsPersonal { get; set; }

		[SchemaProperty("IsCacheable")]
		public bool IsCacheable { get; set; }

		[SchemaProperty("IsSSPAvailable")]
		public bool IsSSPAvailable { get; set; }
	}
}
