namespace ATF.Repository.UnitTests.Models
{
	using System;
	using ATF.Repository.Attributes;

	[Schema("OpportunityContact")]
	public class OpportunityContact: BaseModel
	{
		[SchemaProperty("Opportunity")]
		public Guid OpportunityId { get; set; }

		[LookupProperty("Contact")]
		public virtual Contact Contact { get; set; }

		[SchemaProperty("Role")]
		public Guid RoleId { get; set; }

		[SchemaProperty("Job")]
		public Guid JobId { get; set; }

	}
}
