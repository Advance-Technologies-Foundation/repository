namespace ATF.Repository.Tests.Models
{
	using System;
	using System.Collections.Generic;
	using ATF.Repository.Attributes;

	[Schema("Bonus")]
	public class Bonus : BaseModel
	{
		[SchemaProperty("AccrualDate")]
		public DateTime AccrualDate { get; set; }

		[SchemaProperty("Amount")]
		public decimal Amount { get; set; }

		[SchemaProperty("Comment")]
		public string Comment { get; set; }

		[SchemaProperty("CurrencyRate")]
		public decimal CurrencyRate { get; set; }

		[SchemaProperty("State")]
		public Guid StateId { get; set; }

		[SchemaProperty("Invoice")]
		public Guid InvoiceId { get; set; }

		[ReferenceProperty("InvoiceId")]
		public virtual Invoice Invoice { get; set; }

		[SchemaProperty("Order")]
		public Guid OrderId { get; set; }

		[SchemaProperty("IsTarget")]
		public bool IsTarget { get; set; }

	}
}
