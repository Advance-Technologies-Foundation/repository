namespace ATF.Repository.UnitTests.Models
{
	using ATF.Repository.Attributes;

	[Schema("ProductType")]
	public class ProductType: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}
