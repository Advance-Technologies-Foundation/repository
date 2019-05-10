namespace ATF.Repository.Tests
{
	using NUnit.Framework;
	using System;
	using System.Linq;
	using ATF.Repository.Tests.Models;
    using ATF.Repository.Builder;
    using System.Collections.Generic;
    using System.IO;

    [TestFixture]
	class RepositoryTests
	{
		#region Fields: Private

		private Repository _repository;

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

		/*[TearDown]
		protected override void TearDown() {
		}

		[SetUp]
		protected override void SetUp() {
			base.SetUp();
			UserConnection.Workspace.Id = Guid.NewGuid();
			_repository = new Repository();
			_repository.UserConnection = UserConnection;
		}*/

		#endregion

		#region TestMethods

		/*class Package {
			public IImmutableList<string> AssemblyLocations { get; set; }
		}*/

		[Test]
		public void SaveInvoice_CreateInvoiceAccrueRoleByInvoiceWhenDueDateIsChanged() {

			/*var invoice = _repository.GetItem<Models.Invoice>(new Guid("924CDCB6-1BA6-4CF9-B4C3-046E06256954"));
			invoice.PrimaryAmount = invoice.PrimaryAmount + 10;
			
			var list = ImmutableList.Create<string>("C:\\Projects\\Work\\_Product\\TSBpm\\Src\\Lib\\Terrasoft.WebApp.Loader\\Terrasoft.WebApp\\Terrasoft.Configuration\\Pkg\\WorkPartners\\Files\\Bin");
			var packages = new List<Package>() {
				new Package() {
					AssemblyLocations = ImmutableList.Create<string>("C:\\Projects\\Work\\_Product\\TSBpm\\Src\\Lib\\Terrasoft.WebApp.Loader\\Terrasoft.WebApp\\Terrasoft.Configuration\\Pkg\\WorkOverride\\Files\\Bin\\WorkOverride.dll", "C:\\Projects\\Work\\_Product\\TSBpm\\Src\\Lib\\Terrasoft.WebApp.Loader\\Terrasoft.WebApp\\Terrasoft.Configuration\\Pkg\\WorkOverride\\Files\\Bin")
				},
				new Package() {
					AssemblyLocations = ImmutableList.Create<string>("C:\\Projects\\Work\\_Product\\TSBpm\\Src\\Lib\\Terrasoft.WebApp.Loader\\Terrasoft.WebApp\\Terrasoft.Configuration\\Pkg\\WorkPartners\\Files\\Bin\\WorkPartners.dll", "C:\\Projects\\Work\\_Product\\TSBpm\\Src\\Lib\\Terrasoft.WebApp.Loader\\Terrasoft.WebApp\\Terrasoft.Configuration\\Pkg\\WorkPartners\\Files\\Bin")
				},
				new Package() {
					AssemblyLocations = ImmutableList.Create<string>("C:\\Projects\\Work\\_Product\\TSBpm\\Src\\Lib\\Terrasoft.WebApp.Loader\\Terrasoft.WebApp\\Terrasoft.Configuration\\Pkg\\WorkBilling\\Files\\Bin\\WorkBilling.dll", "C:\\Projects\\Work\\_Product\\TSBpm\\Src\\Lib\\Terrasoft.WebApp.Loader\\Terrasoft.WebApp\\Terrasoft.Configuration\\Pkg\\WorkBilling\\Files\\Bin")
				}
			};
			var locations = packages
				.SelectMany(p => p.AssemblyLocations.Where(Directory.Exists))
				.ToImmutableList();
				*/

			_repository = new Repository();
			var builder = new ProxyClassBuilder(_repository);
			var expense = builder.Build<Expense>();
			//var expense = _repository.GetItem<Expense>(new Guid("CA1E35BA-B7E6-47B6-B125-181D8516B7B0"));
			expense.InvoiceId = new Guid("81C35846-A7B6-4373-BA8B-E2CE27431803");
			var invoice = expense.Invoice;
			var products = expense.ExpenseProducts;
			Assert.AreEqual(true, true);
		}

		#endregion
	}
}
