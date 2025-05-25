namespace ATF.Repository.UnitTests.Models
{
	using ATF.Repository.Attributes;

	[Schema(name: "AccountType")]
	public class AccountType: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}
