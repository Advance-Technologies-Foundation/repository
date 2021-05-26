namespace ATF.Repository.Tests.Models
{
	using ATF.Repository.Attributes;

	[Schema("AccountType")]
	public class AccountType: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}
