namespace ATF.Repository.UnitTests.Models
{
	using System;
	using ATF.Repository.Attributes;

	[Schema("City")]
	public class City: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }

		[SchemaProperty("Region")]
		public Guid RegionId { get; set; }
	}
}
