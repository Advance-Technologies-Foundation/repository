namespace ATF.Repository.Tests.Models
{
	using System;
	using System.Collections.Generic;
	using ATF.Repository;
	using ATF.Repository.Attributes;

	[Schema("TsOrderExpense")]
	public class Expense : BaseModel
	{
		[SchemaProperty("Type")]
		public Guid TypeId { get; set; }

		[SchemaProperty("Partner")]
		public Guid PartnerId { get; set; }

		[SchemaProperty("ExpenseDate")]
		public DateTime ExpenseDate { get; set; }

		[SchemaProperty("Currency")]
		public Guid CurrencyId { get; set; }

		[SchemaProperty("Amount")]
		public decimal Amount { get; set; }

		[SchemaProperty("PrimaryAmount")]
		public decimal PrimaryAmount { get; set; }

		[SchemaProperty("Order")]
		public Guid OrderId { get; set; }

		[SchemaProperty("Invoice")]
		public Guid InvoiceId { get; set; }

		[SchemaProperty("Status")]
		public Guid StatusId { get; set; }

		[SchemaProperty("PrimaryAmountPlan")]
		public decimal PrimaryAmountPlan { get; set; }

		[SchemaProperty("CalculateMethod")]
		public Guid CalculateMethodId { get; set; }

		[DetailProperty("ExpenseId")]
		public virtual List<ExpenseProduct> ExpenseProducts { get; set; }

		[LookupProperty("Invoice")]
		public virtual Invoice Invoice { get; set; }

	}
}
