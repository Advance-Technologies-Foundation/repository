using ATF.Repository.Attributes;

namespace ATF.Repository.UnitTests.Models
{
	[Schema("Country")]
	public class Country: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}
