using System;
using ATF.Repository.Attributes;

namespace ATF.Repository.UnitTests.Models
{
	[Schema("PainChain")]
	public class PainChain: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }

		[SchemaProperty("Lead")]
		public Guid LeadId { get; set; }

		[SchemaProperty("KeyPlayer")]
		public Guid KeyPlayerId { get; set; }
	}
}
