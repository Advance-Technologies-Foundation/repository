namespace ATF.Repository.Tests.Models
{
	using System;
	using ATF.Repository;
	using ATF.Repository.Attributes;

	[Schema("TsOrderExpenseProduct")]
	public class ExpenseProductWithoutMasterColumnLink : BaseModel
	{
		/*[SchemaProperty("TsOrderExpense")]
		public Guid ExpenseId { get; set; }*/

		[SchemaProperty("Amount")]
		public decimal Amount { get; set; }

		[SchemaProperty("CalculateExpense")]
		public bool CalculateExpense { get; set; }

		[LookupProperty("TsOrderExpense")]
		public virtual Expense Expense { get; set; }
	}
}
