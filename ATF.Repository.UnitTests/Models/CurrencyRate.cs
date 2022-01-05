namespace ATF.Repository.UnitTests.Models
{
	using System;
	using ATF.Repository.Attributes;

	[Schema("CurrencyRate")]
	public class CurrencyRate: BaseModel
	{
		[LookupProperty("Currency")]
		public Currency Currency { get; set; }

		[SchemaProperty("EndDate")]
		public DateTime EndDate { get; set; }

		[SchemaProperty("StartDate")]
		public DateTime StartDate { get; set; }

		[SchemaProperty("Rate")]
		public decimal Rate { get; set; }
	}
}
