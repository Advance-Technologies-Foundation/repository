namespace ATF.Repository.UnitTests.Models
{
	using System;
	using System.Collections.Generic;
	using ATF.Repository.Attributes;

	[Schema(name: "Opportunity")]
	public class Opportunity: BaseModel
	{
		[SchemaProperty("Title")]
		public string Title { get; set; }

		[SchemaProperty("Account")]
		public Guid AccountId { get; set; }

		[LookupProperty("Type")]
		public virtual OpportunityType Type { get; set; } //value: "3c3865f2-ada4-480c-ac91-e2d39c5bbaf9", displayValue: "Direct sale"

		[SchemaProperty("Territory")]
		public Guid TerritoryId { get; set; } //value: "e3683f22-cc00-4ecf-ade6-5ba0cea8e39f", displayValue: "Eastern Europe"

		[SchemaProperty("LicenseCount")]
		public int LicenseCount { get; set; }

		[SchemaProperty("LeadType")]
		public Guid LeadTypeId { get; set; }//value: "066dda2c-29ac-4c4c-9ec9-ca1d2ad653f1", displayValue: "BPM"

		[SchemaProperty("DueDate")]
		public DateTime DueDate { get; set; }

		[SchemaProperty("ClosedOnDate")]
		public DateTime ClosedOnDate { get; set; }

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
