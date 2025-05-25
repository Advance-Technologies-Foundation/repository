namespace ATF.Repository.Tests.Models
{
	using System;
	using System.Collections.Generic;
	using ATF.Repository;
	using ATF.Repository.Attributes;

	[Schema(name: "TsOrderExpense")]
	public class Expense : BaseModel
	{
		[SchemaProperty("ExpenseDate")]
		public DateTime ExpenseDate { get; set; }

		[SchemaProperty("Amount")]
		public decimal Amount { get; set; }

		[SchemaProperty("PrimaryAmount")]
		public decimal PrimaryAmount { get; set; }

		[SchemaProperty("Order")]
		public Guid OrderId { get; set; }

		[SchemaProperty("Invoice")]
		public Guid InvoiceId { get; set; }

		[DetailProperty("TsOrderExpense")]
		public virtual List<ExpenseProduct> ExpenseProducts { get; set; }

		[DetailProperty("TsOrderExpense")]
		public virtual List<ExpenseProductWithoutMasterColumnLink> ExpenseProductsWithoutMasterLink { get; set; }

		[LookupProperty("Order")]
		public virtual Order Order { get; set; }

		[LookupProperty("Invoice")]
		public virtual Invoice Invoice { get; set; }

	}
}
