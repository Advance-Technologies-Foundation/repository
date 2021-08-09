using System;
using System.Collections.Generic;
using ATF.Repository.Attributes;

namespace ATF.Repository.UnitTests.Models
{
	[Schema("TypedTestModel")]
	public class TypedTestModel: BaseModel
	{
		[SchemaProperty("StringValue")]
		public string StringValue { get; set; }

		[SchemaProperty("IntValue")]
		public int IntValue { get; set; }

		[SchemaProperty("DecimalValue")]
		public decimal DecimalValue { get; set; }

		[SchemaProperty("DateTimeValue")]
		public DateTime DateTimeValue { get; set; }

		[SchemaProperty("GuidValue")]
		public Guid GuidValueId { get; set; }

		[SchemaProperty("BooleanValue")]
		public bool BooleanValue { get; set; }

		[SchemaProperty("LookupValue")]
		public Guid LookupValueId { get; set; }

		[LookupProperty("LookupValue")]
		public virtual LookupTestModel LookupValue { get; set; }

		[LookupProperty("AnotherLookupValue")]
		public virtual LookupTestModel AnotherLookupValue { get; set; }

		[LookupProperty("Parent")]
		public virtual TypedTestModel Parent { get; set; }

		[DetailProperty("GuidValueId")]
		public virtual List<TypedTestModel> DetailModels { get; set; }

		[DetailProperty("GuidValue")]
		public virtual List<TypedTestModel> AnotherDetailModels { get; set; }
	}
}
