namespace ATF.Repository.UnitTests.Models
{
	using ATF.Repository.Attributes;

	[Schema("OpportunityType")]
	public class OpportunityType: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}
