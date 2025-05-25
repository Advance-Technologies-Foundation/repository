namespace ATF.Repository.UnitTests.Models
{
	using System;
	using ATF.Repository.Attributes;

	[Schema(name: "City")]
	public class City: BaseModel
	{
		[SchemaProperty("CreatedOn")]
		public DateTime CreatedOn { get; set; }

		[SchemaProperty("Name")]
		public string Name { get; set; }

		[SchemaProperty("Region")]
		public Guid RegionId { get; set; }
	}
}
