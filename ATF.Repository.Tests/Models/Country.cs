using ATF.Repository.Attributes;

namespace ATF.Repository.Tests.Models
{
	[Schema("Country")]
	public class Country: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}
