namespace ATF.Repository.UnitTests.Models
{
	using ATF.Repository.Attributes;

	[Schema("ProductSource")]
	public class ProductSource: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}
