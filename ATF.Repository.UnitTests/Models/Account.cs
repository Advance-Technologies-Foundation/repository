namespace ATF.Repository.UnitTests.Models
{
	using System;
	using System.Collections.Generic;
	using ATF.Repository.Attributes;

	[Schema(name: "Account")]
	public class Account: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }

		[SchemaProperty("AccountCategory")]
		public Guid AccountCategoryId { get; set; }

		[SchemaProperty("ExactNoOfEmployees")]
		public int ExactNoOfEmployees { get; set; }

		[LookupProperty("PrimaryContact")]
		public virtual Contact PrimaryContact { get; set; }

		[DetailProperty("AccountId")]
		public virtual List<Contact> Contacts { get; set; }

		[LookupProperty("MasterAccount")]
		public virtual Account MasterAccount { get; set; }
	}
}
