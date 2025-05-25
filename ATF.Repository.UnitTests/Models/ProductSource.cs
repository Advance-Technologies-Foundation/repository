namespace ATF.Repository.UnitTests.Models
{
	using ATF.Repository.Attributes;

	[Schema(name: "ProductSource")]
	public class ProductSource: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}
