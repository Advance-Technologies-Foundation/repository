using System;
using ATF.Repository.Attributes;

namespace ATF.Repository.UnitTests.Models
{
	[Schema("City")]
	public class City: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }

		[SchemaProperty("Region")]
		public Guid RegionId { get; set; }
	}
}
