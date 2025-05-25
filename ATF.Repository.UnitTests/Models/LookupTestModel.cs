namespace ATF.Repository.UnitTests.Models
{
	using ATF.Repository.Attributes;

	[Schema(name: "LookupTestModel")]
	public class LookupTestModel: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}
