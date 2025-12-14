namespace ATF.Repository.UnitTests.Models
{
	using System;
	using System.Collections.Generic;
	using ATF.Repository.Attributes;

	[Schema("AtfOpportunity")]
	public class Opportunity: BaseModel
	{
		[SchemaProperty("Title")]
		public string Title { get; set; }

		[SchemaProperty("Account")]
		public Guid AccountId { get; set; }

		[SchemaProperty("Type")]
		public Guid TypeId { get; set; }

		[LookupProperty("Type")]
		public virtual AccountType Type { get; set; }

		[SchemaProperty("LicenseCount")]
		public int LicenseCount { get; set; }

		[SchemaProperty("DueDate")]
		public DateTime DueDate { get; set; }

		[SchemaProperty("IsPrimary")]
		public bool IsPrimary { get; set; }

		[SchemaProperty("Budget")]
		public decimal Budget { get; set; }

		[SchemaProperty("CreatedOn")]
		public DateTime CreatedOn { get; set; }

		[DetailProperty("OpportunityId")]
		public virtual List<OpportunityContact> OpportunityContacts { get; set; }
	}
}
