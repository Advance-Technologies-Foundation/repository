namespace ATF.Repository.UnitTests.Models
{
	using System;
	using ATF.Repository.Attributes;

	[Schema("AtfOpportunityContact")]
	public class OpportunityContact: BaseModel
	{
		[SchemaProperty("Opportunity")]
		public Guid OpportunityId { get; set; }

		[SchemaProperty("Contact")]
		public Guid ContactId { get; set; }

	}
}
