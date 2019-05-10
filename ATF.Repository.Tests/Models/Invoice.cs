namespace ATF.Repository.Tests.Models
{
	using System;
	using System.Collections.Generic;
	using ATF.Repository.Attributes;

	[Schema("Invoice")]
	public class Invoice : BaseModel
	{
		[SchemaProperty("PrimaryAmount")]
		public decimal PrimaryAmount { get; set; }

		[SchemaProperty("CurrencyRate")]
		public decimal CurrencyRate { get; set; }

		[SchemaProperty("PaymentCurrencyRate")]
		public decimal PaymentCurrencyRate { get; set; }

		[SchemaProperty("Order")]
		public Guid OrderId { get; set; }

		[DetailProperty("InvoiceId")]
		public virtual List<Expense> Expenses { get; set; }

	}
}
