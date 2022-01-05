namespace ATF.Repository.Tests
{
	using NUnit.Framework;
	using System;
	using System.Linq;
	using Models;
	using System.Collections.Generic;
	using Terrasoft.Configuration.Tests;

	[TestFixture]
	public class RepositoryWithoutUserConnectionTests : BaseConfigurationTestFixture
	{
		#region Fields: Private

		private Repository _repository;

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
			var expense = _repository.CreateItem<Expense>();
			var invoice = _repository.CreateItem<Invoice>();
			expense.InvoiceId = invoice.Id;
			Assert.AreEqual(invoice, expense.Invoice);
		}

		[Test]
		public void LookupProperty_WithExternalValueAndExternalValueIsEmpty_ShouldReturnNull() {
			var expense = _repository.CreateItem<Expense>();
			Assert.AreEqual(null, expense.Invoice);
		}

		[Test]
		public void ReferenceProperty_WhenExternalValueIdIsNull_ShouldReturnNull() {
			var expenseProduct = _repository.CreateItem<ExpenseProduct>();
			Assert.AreEqual(null, expenseProduct.Expense);
		}

		[Test]
		public void ReferenceProperty_WhenExternalValueIdIsNotNull_ShouldReturnModel() {
			var expenseProduct = _repository.CreateItem<ExpenseProduct>();
			var expense = _repository.CreateItem<Expense>();
			expenseProduct.ExpenseId = expense.Id;
			Assert.AreEqual(expense, expenseProduct.Expense);
		}

		[Test]
		public void DetailProperty_ShouldReturnEmptyList() {
			var expense = _repository.CreateItem<Expense>();
			Assert.AreNotEqual(null, expense.ExpenseProducts);
			Assert.AreEqual(typeof(List<ExpenseProduct>), expense.ExpenseProducts.GetType());
			Assert.AreEqual(0, expense.ExpenseProducts.Count);
		}

		[Test]
		public void ChangeTracker_GetItems_WithoutTyped_ShouldReturnsExpectedValue() {
			var firstOrder = _repository.CreateItem<Order>();
			var secondOrder = _repository.CreateItem<Order>();
			var invoice = _repository.CreateItem<Invoice>();
			var trackedModels = _repository.ChangeTracker.GetTrackedModels();

			var enumerable = trackedModels as ITrackedModel<BaseModel>[] ?? trackedModels.ToArray();
			Assert.IsTrue(enumerable.Any(x => x.Model == firstOrder));
			Assert.IsTrue(enumerable.Any(x => x.Model == secondOrder));
			Assert.IsTrue(enumerable.Any(x => x.Model == invoice));
			Assert.AreEqual(3, enumerable.Length);
		}

		[Test]
		public void ChangeTracker_GetItems_WithTyped_ShouldReturnsExpectedValue() {
			var firstOrder = _repository.CreateItem<Order>();
			var secondOrder = _repository.CreateItem<Order>();
			var invoice = _repository.CreateItem<Invoice>();
			var trackedModels = _repository.ChangeTracker.GetTrackedModels<Order>();

			var enumerable = trackedModels as ITrackedModel<Order>[] ?? trackedModels.ToArray();
			Assert.IsTrue(enumerable.Any(x => x.Model == firstOrder));
			Assert.IsTrue(enumerable.Any(x => x.Model == secondOrder));
			Assert.IsFalse(enumerable.Any(x => x.Model == (BaseModel)invoice));
			Assert.AreEqual(2, enumerable.Length);
		}

		#endregion
	}
}
