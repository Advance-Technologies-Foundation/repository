namespace ATF.Repository.UnitTests.Models
{
	using System;
	using System.Collections.Generic;
	using ATF.Repository.Attributes;

	[Schema(name: "Contact")]
	public class Contact: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }

		[SchemaProperty("Email")]
		public string Email { get; set; }

		[SchemaProperty("Age")]
		public int Age { get; set; }

		[SchemaProperty("Account")]
		public Guid AccountId { get; set; }

		[LookupProperty("Account")]
		public virtual Account Account { get; set; }

		[SchemaProperty("Type")]
		public Guid TypeId { get; set; }

		[SchemaProperty("ContactSource")]
		public Guid ContactSourceId { get; set; }

		[SchemaProperty("Phone")]
		public string Phone { get; set; }

		[DetailProperty("EntityId")]
		public virtual List<ContactInTag> ContactInTags { get; set; }

	}
}
