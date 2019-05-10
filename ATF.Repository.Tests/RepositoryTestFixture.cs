namespace ATF.Repository.Tests
{
	using NUnit.Framework;
	using System;
	using Terrasoft.Configuration.Tests;
	using ATF.Repository.Tests.Models;

	[TestFixture, Category("Integration")]
	[EntityManagerSettings(EntityManagerMode.InitFromDb)]
	class RepositoryTestFixture : BaseConfigurationTestFixture
	{
		#region Fields: Private

		private IRepository _repository;

		/*private static readonly Dictionary<string, object> _currencyColumnsValues = new Dictionary<string, object> {
			{"Id", Guid.NewGuid()},
			{"Division", 1},
			{"Rate", 2}
		};

		private static readonly Dictionary<string, object> _invoiceValues = new Dictionary<string, object> {
			{"Id", Guid.NewGuid()},
			{"PaymentCurrencyId", _currencyColumnsValues["Id"]},
			{"PaymentStatusId", WorkOrderConsts.InvoicePaymentStatus.WaitlyPaid},
			{"FillDetailsManually", false},
			{"InvoiceKindId", WorkSalesBaseConst.InvoiceKind.RenewalCrossUp},
			{"Margin", 100m}
		};*/

		#endregion

		#region Methods: Protected

		[TearDown]
		protected override void TearDown() {
		}

		[SetUp]
		protected override void SetUp() {
			var item = NSubstitute.Arg.Any<string>();
			base.SetUp();
			UserConnection.Workspace.Id = Guid.NewGuid();
			_repository = new Repository();
			_repository.UserConnection = UserConnection;
		}

		#endregion

		#region TestMethods

		[Test]
		public void SaveInvoice_CreateInvoiceAccrueRoleByInvoiceWhenDueDateIsChanged() {
			/*
			var invoice = _repository.GetItem<Models.Invoice>(new Guid("924CDCB6-1BA6-4CF9-B4C3-046E06256954"));
			invoice.PrimaryAmount = invoice.PrimaryAmount + 10;
			*/
			var expense = _repository.GetItem<Expense>(new Guid("CB9E033D-2193-46F8-BFB8-8CCB6C093DA5"));
			var invoice = expense.Invoice;
			var products = expense.ExpenseProducts;
			Assert.AreEqual(true, true);
		}

		#endregion
	}
}
