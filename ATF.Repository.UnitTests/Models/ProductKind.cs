namespace ATF.Repository.UnitTests.Models
{
	using ATF.Repository.Attributes;

	[Schema("ProductKind")]
	public class ProductKind: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}
