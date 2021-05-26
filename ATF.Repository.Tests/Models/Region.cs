using System.Collections.Generic;
using ATF.Repository.Attributes;

namespace ATF.Repository.Tests.Models
{
	[Schema("Region")]
	public class Region: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }

		[LookupProperty("Country")]
		public Country Country { get; set; }

		[DetailProperty("Region")]
		public List<City> Cities { get; set; }
	}
}
