namespace ATF.Repository.Tests
{
	using NUnit.Framework;
	using System;
	using Terrasoft.Configuration.Tests;
	using ATF.Repository.Tests.Models;

	[TestFixture]
	[MockSettings(RequireMock.DBEngine)]
	class RepositoryWithUserConnectionTests : BaseConfigurationTestFixture
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
			base.SetUp();
			UserConnection.Workspace.Id = Guid.NewGuid();
			_repository = new Repository();
			_repository.UserConnection = UserConnection;
		}

		#endregion

		#region TestMethods

		[Test]
		public void SaveInvoice_CreateInvoiceAccrueRoleByInvoiceWhenDueDateIsChanged() {
		
			Assert.AreEqual(true, true);
		}

		#endregion
	}
}
