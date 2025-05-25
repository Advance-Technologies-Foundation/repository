namespace ATF.Repository.UnitTests.Models
{
	using ATF.Repository.Attributes;

	[Schema(name: "ProductCategory")]
	public class ProductCategory: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}
