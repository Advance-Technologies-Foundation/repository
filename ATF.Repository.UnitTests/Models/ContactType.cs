namespace ATF.Repository.UnitTests.Models
{
	using ATF.Repository.Attributes;
	using Terrasoft.Common;

	[Schema(name: "ContactType")]
	public class ContactType: BaseModel
	{
		[SchemaProperty("Name")]
		public LocalizableString Name { get; set; }
	}
}
