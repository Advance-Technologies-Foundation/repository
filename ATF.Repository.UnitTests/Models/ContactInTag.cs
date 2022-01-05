namespace ATF.Repository.UnitTests.Models
{
	using System;
	using ATF.Repository.Attributes;

	[Schema("ContactInTag")]
	public class ContactInTag: BaseModel
	{
		[SchemaProperty("Entity")]
		public Guid EntityId { get; set; }

		[LookupProperty("Entity")]
		public Contact Entity { get; set; }

		[SchemaProperty("Tag")]
		public Guid TagId { get; set; }
	}
}
