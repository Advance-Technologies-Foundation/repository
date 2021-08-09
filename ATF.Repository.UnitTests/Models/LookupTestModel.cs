using ATF.Repository.Attributes;

namespace ATF.Repository.UnitTests.Models
{
	[Schema("LookupTestModel")]
	public class LookupTestModel: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}
