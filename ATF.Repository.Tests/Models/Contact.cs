using System;
using ATF.Repository.Attributes;

namespace ATF.Repository.Tests.Models
{
	[Schema("Contact")]
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

	}
}
