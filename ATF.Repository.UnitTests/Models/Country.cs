namespace ATF.Repository.UnitTests.Models
{
	using ATF.Repository.Attributes;

	[Schema("Country")]
	public class Country: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}
