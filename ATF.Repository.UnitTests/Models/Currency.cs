namespace ATF.Repository.UnitTests.Models
{
	using ATF.Repository.Attributes;

	[Schema(name: "Currency")]
	public class Currency: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}
