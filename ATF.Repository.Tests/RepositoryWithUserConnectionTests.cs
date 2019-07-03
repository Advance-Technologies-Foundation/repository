namespace ATF.Repository.Tests
{
	using NUnit.Framework;
	using System;
	using Terrasoft.Configuration.Tests;
	using ATF.Repository.Tests.Models;
	using System.Collections.Generic;
	using Terrasoft.Common;
	using System.Linq;

	[TestFixture]
	[MockSettings(RequireMock.DBEngine)]
	class RepositoryWithUserConnectionTests : BaseConfigurationTestFixture
	{
		#region Fields: Private

		private IRepository _repository;
		private static DateTime _testDateTime = DateTime.Now;

		private static readonly Dictionary<string, object> _orderValues = new Dictionary<string, object> {
			{ "Id", Guid.NewGuid() },
			{ "Number", "TestOrderNumber" }
		};

		private static readonly Dictionary<string, object> _invoiceValues = new Dictionary<string, object> {
			{ "Id", Guid.NewGuid() },
			{ "OrderId", _orderValues["Id"] },
			{ "PrimaryAmount", 1000m },
			{ "CurrencyRate", 1m },
			{ "PaymentCurrencyRate", 1m },
		};

		private static readonly Dictionary<string, object> _expense1Values = new Dictionary<string, object> {
			{ "Id", Guid.NewGuid() },
			{ "Amount",  100m },
			{ "PrimaryAmount", 100m },
			{ "OrderId", _orderValues["Id"] },
			{ "InvoiceId", _invoiceValues["Id"] },
			{ "ExpenseDate", _testDateTime }
		};

		private static readonly Dictionary<string, object> _expense2Values = new Dictionary<string, object> {
			{ "Id", Guid.NewGuid() },
			{ "Amount",  150m },
			{ "PrimaryAmount", 150m },
			{ "OrderId", _orderValues["Id"] },
			{ "InvoiceId", _invoiceValues["Id"] },
			{ "ExpenseDate", _testDateTime }
		};

		private static readonly Dictionary<string, object> _expense1product1Values = new Dictionary<string, object> {
			{ "Id", Guid.NewGuid() },
			{ "Amount", 40m },
			{ "CalculateExpense", false },
			{ "TsOrderExpenseId", _expense1Values["Id"] }
		};

		private static readonly Dictionary<string, object> _expense1product2Values = new Dictionary<string, object> {
			{ "Id", Guid.NewGuid() },
			{ "Amount", 60m },
			{ "CalculateExpense", false },
			{ "TsOrderExpenseId", _expense1Values["Id"] }
		};

		private void AddCustomizedEntitySchemas() {
			EntitySchemaManager.AddCustomizedEntitySchema("Order", new Dictionary<string, string> {
				{ "Number", "ShortText" },
			});

			var invoiceSchema = EntitySchemaManager.AddCustomizedEntitySchema("Invoice", new Dictionary<string, string> {
				{ "PrimaryAmount", "Float2" },
				{ "CurrencyRate", "Float2" },
				{ "PaymentCurrencyRate", "Float2" },
			});
			invoiceSchema.AddLookupColumn("Order", "Order");

			var tsOrderExpenseSchema = EntitySchemaManager.AddCustomizedEntitySchema("TsOrderExpense", new Dictionary<string, string> {
				{ "Amount", "Float2" },
				{ "ExpenseDate", "DateTime" },
				{ "PrimaryAmount", "Float2" },
			});
			tsOrderExpenseSchema.AddLookupColumn("Order", "Order");
			tsOrderExpenseSchema.AddLookupColumn("Invoice", "Invoice");

			var tsOrderExpenseProductSchema = EntitySchemaManager.AddCustomizedEntitySchema("TsOrderExpenseProduct", new Dictionary<string, string> {
				{ "Amount", "Float2" },
				{ "CalculateExpense", "Boolean" }
			});
			tsOrderExpenseProductSchema.AddLookupColumn("TsOrderExpense", "TsOrderExpense");
		}

		private void SetUpTestData() {
			SetUpTestData("Order", _orderValues);
			SetUpTestData("Invoice", _invoiceValues);
			SetUpTestData("TsOrderExpense", _expense1Values, _expense2Values);
			SetUpTestData("TsOrderExpenseProduct", _expense1product1Values, _expense1product2Values);
		}

		private void SetUpTestData(string schemaName, params Dictionary<string, object>[] items) {
			var selectData = new SelectData(UserConnection, schemaName);
			items.ForEach(values => selectData.AddRow(values));
			selectData.MockUp();
		}

		#endregion

		#region Methods: Protected

		[TearDown]
		protected override void TearDown() {
		}

		[SetUp]
		protected override void SetUp() {
			base.SetUp();
			UserConnection.Workspace.Id = Guid.NewGuid();
			_repository = new Repository();
			_repository.UserConnection = UserConnection;
			AddCustomizedEntitySchemas();
			SetUpTestData();
		}

		#endregion

		#region TestMethods

		[Test]
		public void GetStringProperty_ShouldEqualsExpectedValue() {
			var order = _repository.GetItem<Order>((Guid)_orderValues["Id"]);
			Assert.AreEqual(_orderValues["Number"], order.Number);
		}

		[Test]
		public void GetDateTimeProperty_ShouldEqualsExpectedValue() {
			var expense = _repository.GetItem<Expense>((Guid)_expense1Values["Id"]);
			Assert.AreEqual(_testDateTime, expense.ExpenseDate);
		}

		[Test]
		public void GetLookupPropertyWithSchemaProperty_ShouldEqualsExpectedValue() {
			var expense = _repository.GetItem<Expense>((Guid)_expense1Values["Id"]);
			Assert.AreEqual(_orderValues["Id"], expense.Order.Id);
		}

		[Test]
		public void GetReferenceProperty_ShouldEqualsExpectedValue() {
			var expense = _repository.GetItem<Expense>((Guid)_expense1Values["Id"]);
			Assert.AreEqual(_invoiceValues["Id"], expense.Invoice.Id);
		}

		[Test]
		public void GetDecimalProperty_ShouldEqualsExpectedValue() {
			var expense = _repository.GetItem<Expense>((Guid)_expense1Values["Id"]);
			Assert.AreEqual(_expense1Values["Amount"], expense.Amount);
		}

		[Test]
		public void GetDetailProperty_ShouldEqualsExpectedValue() {
			var expense = _repository.GetItem<Expense>((Guid)_expense1Values["Id"]);
			Assert.AreEqual(2, expense.ExpenseProducts.Count);
		}

		[Test]
		public void GetDetailPropertyWithValues_ShouldEqualsExpectedValue() {
			var expense = _repository.GetItem<Expense>((Guid)_expense1Values["Id"]);
			Assert.AreEqual(100, expense.ExpenseProducts.Sum(x => x.Amount));
		}

		#endregion
	}
}
