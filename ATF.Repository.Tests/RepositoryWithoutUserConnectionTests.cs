namespace ATF.Repository.Tests
{
	using NUnit.Framework;
	using System;
	using System.Linq;
	using ATF.Repository.Tests.Models;
	using ATF.Repository.Builder;
	using System.Collections.Generic;
	using System.IO;
	using Terrasoft.Configuration.Tests;

	[TestFixture]
	class RepositoryWithoutUserConnectionTests : BaseConfigurationTestFixture
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

		[TearDown]
		protected override void TearDown() {
		}

		[SetUp]
		protected override void SetUp() {
			base.SetUp();
			UserConnection.Workspace.Id = Guid.NewGuid();
			_repository = new Repository();
		}

		#endregion

		#region TestMethods

		[Test]
		public void GetItem_ShouldBeNull() {
			var invoice = _repository.GetItem<Invoice>(Guid.NewGuid());
			Assert.AreEqual(null, invoice);
		}

		[Test]
		public void GetItem_ShouldReturnModel() {
			var model = _repository.CreateItem<Invoice>();
			var invoice = _repository.GetItem<Invoice>(model.Id);
			Assert.AreEqual(model, invoice);
		}

		[Test]
		public void LookupPropertyWithoutExternalValue_ShouldBeNull() {
			var model = _repository.CreateItem<Invoice>();
			var invoice = _repository.GetItem<Invoice>(model.Id);
			Assert.AreEqual(null, invoice.Order);
		}

		[Test]
		public void LookupProperty_WithExternalValueAndExternalValueIsNotEmpty_ShouldReturnModel() {
			var expence = _repository.CreateItem<Expense>();
			var invoice = _repository.CreateItem<Invoice>();
			expence.InvoiceId = invoice.Id;
			Assert.AreEqual(invoice, expence.Invoice);
		}

		[Test]
		public void LookupProperty_WithExternalValueAndExternalValueIsEmpty_ShouldReturnNull() {
			var expence = _repository.CreateItem<Expense>();
			Assert.AreEqual(null, expence.Invoice);
		}

		[Test]
		public void ReferenceProperty_WhenExternalValueIdIsNull_ShouldReturnNull() {
			var expenseProduct = _repository.CreateItem<ExpenseProduct>();
			Assert.AreEqual(null, expenseProduct.Expense);
		}

		[Test]
		public void ReferenceProperty_WhenExternalValueIdIsNotNull_ShouldReturnModel() {
			var expenseProduct = _repository.CreateItem<ExpenseProduct>();
			var expence = _repository.CreateItem<Expense>();
			expenseProduct.ExpenseId = expence.Id;
			Assert.AreEqual(expence, expenseProduct.Expense);
		}

		[Test]
		public void DetailProperty_ShouldReturnEmptyList() {
			var expence = _repository.CreateItem<Expense>();
			Assert.AreNotEqual(null, expence.ExpenseProducts);
			Assert.AreEqual(typeof(List<ExpenseProduct>), expence.ExpenseProducts.GetType());
			Assert.AreEqual(0, expence.ExpenseProducts.Count);
		}

		#endregion
	}
}
