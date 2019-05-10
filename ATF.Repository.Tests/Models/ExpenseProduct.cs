namespace ATF.Repository.Tests.Models
{
	using System;
	using ATF.Repository;
	using ATF.Repository.Attributes;

	[Schema("TsOrderExpenseProduct")]
	public class ExpenseProduct : BaseModel
	{
		[SchemaProperty("OrderProduct")]
		public Guid OrderProductId { get; set; }

		[SchemaProperty("TsOrderExpense")]
		public Guid ExpenseId { get; set; }

		[SchemaProperty("InvoiceProduct")]
		public Guid InvoiceProductId { get; set; }

		[SchemaProperty("CalculateMethod")]
		public Guid CalculateMethodId { get; set; }

		[SchemaProperty("Amount")]
		public decimal Amount { get; set; }

		[SchemaProperty("CalculateExpense")]
		public bool CalculateExpense { get; set; }

		[SchemaProperty("PrimaryAmount")]
		public decimal PrimaryAmount { get; set; }

		[ReferenceProperty("ExpenseId")]
		public virtual Expense Expense { get; set; }
	}
}
