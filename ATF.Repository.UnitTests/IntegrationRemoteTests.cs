namespace ATF.Repository.UnitTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.Providers;
	using ATF.Repository.UnitTests.Models;
	using NUnit.Framework;

	[TestFixture]
	public class IntegrationRemoteTests : BaseIntegrationTests
	{
		private IDataProvider _dataProvider;
		private IAppDataContext _appDataContext;
		private IAppDataContext _secondaryAppDataContext;

		[SetUp]
		public void SetUp() {
			_dataProvider = GetIntegrationDataProvider();
			_appDataContext = AppDataContextFactory.GetAppDataContext(_dataProvider);
			_secondaryAppDataContext = AppDataContextFactory.GetAppDataContext(_dataProvider);
		}

		[Test]
		public void Models_WhenCallWithoutAnyFilters_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<AccountType>().ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.GreaterOrEqual(models.Count, 0);
			Assert.IsTrue(models.All(x=>x.Id != Guid.Empty));
			Assert.IsTrue(models.All(x=>!string.IsNullOrEmpty(x.Name)));
		}

		[Test]
		public void Models_WhenCallWithoutAnyFilters_ShouldReturnsLessOrEqualMaxCountModels() {
			// Act
			var models = _appDataContext.Models<City>().ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.LessOrEqual(models.Count, 20000);
		}

		[Test]
		public void Models_WhenUseGuidFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Contact>().Where(x=> x.TypeId == new Guid("60733efc-f36b-1410-a883-16d83cab0980")).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.IsTrue(models.All(x=>x.Id != Guid.Empty));
			Assert.IsTrue(models.All(x=>!string.IsNullOrEmpty(x.Name)));
		}

		[Test]
		public void Models_WhenUseIntegerEqualFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.AnnualRevenueBC == 0).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual(0, models.First().AnnualRevenueBC);
		}

		[Test]
		public void Models_WhenUseIntegerNotEqualFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Account>().Where(x=> x.Completeness != 0).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreNotEqual(0, models.First().Completeness);
		}

		[Test]
		public void Models_WhenUseIntegerGreaterFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Account>().Where(x=> x.Completeness > 0).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.Greater(models.First().Completeness, 0);
		}

		[Test]
		public void Models_WhenUseIntegerGreaterOrEqualFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Account>().Where(x=> x.Completeness >= 0).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.GreaterOrEqual(models.First().Completeness, 0);
		}

		[Test]
		public void Models_WhenUseIntegerLessFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.AnnualRevenueBC < 10).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.Less(models.First().AnnualRevenueBC, 10);
		}

		[Test]
		public void Models_WhenUseIntegerLessOrEqualFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.AnnualRevenueBC <= 10).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.LessOrEqual(models.First().AnnualRevenueBC, 10);
		}

		[Test]
		public void Models_WhenUseDecimalGreaterFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.AnnualRevenueBC > 0).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.IsTrue(models.All(x=>x.Id != Guid.Empty));
			Assert.IsTrue(models.All(x=>x.AnnualRevenueBC > 0));
		}

		[Test]
		public void Models_WhenUseDecimalLessFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.AnnualRevenueBC < 1000).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.IsTrue(models.All(x=>x.Id != Guid.Empty));
			Assert.IsTrue(models.All(x=>x.AnnualRevenueBC < 1000));
		}

		[Test]
		public void Models_WhenUseDecimalEqualFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.AnnualRevenueBC == 0).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual(0, models.First().AnnualRevenueBC);
		}

		[Test]
		public void Models_WhenUseDecimalNotEqualFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.AnnualRevenueBC != 0).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreNotEqual(0, models.First().AnnualRevenueBC);
		}

		[Test]
		public void Models_WhenUseDecimalGreaterOrEqualFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.AnnualRevenueBC >= 0).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.GreaterOrEqual(models.First().AnnualRevenueBC, 0);
		}

		[Test]
		public void Models_WhenUseDecimalLessOrEqualFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.AnnualRevenueBC <= 1000).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.LessOrEqual(models.First().AnnualRevenueBC, 1000);
		}

		[Test]
		public void Models_WhenUseStringFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Contact>().Where(x=> x.Name == "Supervisor").Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual("supervisor", models.First().Name.ToLower());
		}

		[Test]
		public void Models_WhenUseStringIsNullFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Contact>().Where(x=> x.Email == null).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.IsNull(models.First().Email);
		}

		[Test]
		public void Models_WhenUseStringStartWithFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Contact>().Where(x=> x.Name.StartsWith("Supervis")).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual("supervisor", models.First().Name.ToLower());
		}

		[Test]
		public void Models_WhenUseStringEndWithFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Contact>().Where(x=> x.Name.EndsWith("rvisor")).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual("supervisor", models.First().Name.ToLower());
		}

		[Test]
		public void Models_WhenUseStringContainsFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Contact>().Where(x=> x.Name.Contains("uperviso")).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.IsTrue(models.First().Name.Contains("uperviso"));
		}

		[Test]
		public void Models_WhenUseBoolShortTrueFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.IsTrialConfirmed).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual(true, models.First().IsTrialConfirmed);
		}

		[Test]
		public void Models_WhenUseBoolShortNotTrueFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> !x.IsTrialConfirmed).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual(false, models.First().IsTrialConfirmed);
		}

		[Test]
		public void Models_WhenUseBoolLongTrueFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.IsTrialConfirmed == true).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual(true, models.First().IsTrialConfirmed);
		}

		[Test]
		public void Models_WhenUseBoolLongNotTrueFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.IsTrialConfirmed != true).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual(false, models.First().IsTrialConfirmed);
		}

		[Test]
		public void Models_WhenUseBoolLongFalseFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.IsTrialConfirmed == false).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual(false, models.First().IsTrialConfirmed);
		}

		[Test]
		public void Models_WhenUseDateTimeEqualNullFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.DecisionDate == null).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual(false, models.First().IsTrialConfirmed);
		}

		[Test]
		public void Models_WhenUseDateTimeEqualNotNullFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.DecisionDate != null).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual(false, models.First().IsTrialConfirmed);
		}

		[Test]
		public void Models_WhenUseDateTimeGreaterFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.DecisionDate > new DateTime(2010, 1, 1)).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual(false, models.First().IsTrialConfirmed);
		}

		[Test]
		public void Models_WhenUseDateTimeLessFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Lead>().Where(x=> x.DecisionDate < new DateTime(2030, 1, 1)).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual(false, models.First().IsTrialConfirmed);
		}

		[Test]
		public void Models_WhenUseLinkedFilter_ShouldReturnExpectedValue() {
			var typeId = new Guid("60733efc-f36b-1410-a883-16d83cab0980");

			// Act
			var models = _appDataContext.Models<Account>().Where(x => x.PrimaryContact.TypeId == typeId).ToList();

			// Assert
			Assert.IsTrue(models.All(x=>x.PrimaryContact.TypeId == typeId));
		}

		[Test]
		public void Models_WhenUseFirstOrDefaultWithFilter_ShouldReturnExpectedValue() {
			// Act
			var model = _appDataContext.Models<Contact>().FirstOrDefault(x =>
				x.Name == "Supervisor" && x.Id == new Guid("410006e1-ca4e-4502-a9ec-e54d922d2c00"));

			// Assert
			Assert.IsNotNull(model);
			Assert.AreEqual("Supervisor", model.Name);
			Assert.AreEqual(new Guid("410006e1-ca4e-4502-a9ec-e54d922d2c00"), model.Id);
		}

		[Test]
		public void Models_WhenUseContainsStringFilter_ShouldReturnExpectedValue() {
			// Arrange
			var names = new List<string>() {"supervisor", "manager"};

			// Act
			var models = _appDataContext.Models<Contact>().Where(x => names.Contains(x.Name)).ToList();

			// Assert
			Assert.IsNotNull(models);
			models.ForEach(x => {
				Assert.Contains(x.Name.ToLower(), names);
			});
		}

		[Test]
		public void Models_WhenUseNotContainsStringFilter_ShouldReturnExpectedValue() {
			// Arrange
			var names = new List<string>() {"supervisor", "manager"};

			// Act
			var models = _appDataContext.Models<Contact>().Where(x => !names.Contains(x.Name)).Take(10).ToList();

			// Assert
			Assert.IsNotNull(models);
			models.ForEach(x => {
				Assert.IsTrue(!names.Contains(x.Name.ToLower()));
			});
		}

		[Test]
		public void Models_WhenUseContainsGuidFilter_ShouldReturnExpectedValue() {
			// Arrange
			var types = new List<Guid>() {new Guid("2b6b75b6-d794-47bf-b5df-31dd95aa012d"), new Guid("be4dc5a1-88c7-493f-8c40-b70fd769a745")};

			// Act
			var models = _appDataContext.Models<Contact>().Where(x => types.Contains(x.TypeId)).ToList();

			// Assert
			Assert.IsNotNull(models);
			models.ForEach(x => {
				Assert.Contains(x.TypeId, types);
			});
		}

		[Test]
		public void Models_WhenUseNotContainsGuidFilter_ShouldReturnExpectedValue() {
			// Arrange
			var types = new List<Guid>() {new Guid("2b6b75b6-d794-47bf-b5df-31dd95aa012d"), new Guid("be4dc5a1-88c7-493f-8c40-b70fd769a745")};

			// Act
			var models = _appDataContext.Models<Contact>().Where(x => !types.Contains(x.TypeId)).Take(10).ToList();

			// Assert
			Assert.IsNotNull(models);
			models.ForEach(x => {
				Assert.IsTrue(!types.Contains(x.TypeId));
			});
		}

		[Test]
		public void Models_WhenGetItemsTwoTimes_ShouldReturnsTheSameItem() {
			// Arrange
			var expectedValue = "expectedValue";
			var itemId = new Guid("410006e1-ca4e-4502-a9ec-e54d922d2c00");

			// Act
			var model = _appDataContext.Models<Contact>().FirstOrDefault(x => x.Id == itemId);

			// Assert
			Assert.IsNotNull(model);
			model.Phone = expectedValue;
			var model2 = _appDataContext.Models<Contact>().FirstOrDefault(x => x.Id == itemId);
			Assert.AreEqual(expectedValue, model2.Phone);
		}

		[Test]
		public void Models_WhenCallModelWithNoLazyLookupProperty_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.Models<Region>().FirstOrDefault(x => x.Id == new Guid("90EDC2EE-07DD-DF11-971B-001D60E938C6"));

			// Assert
			Assert.IsNotNull(model);
			Assert.IsNotNull(model.Country);
		}

		[Test]
		public void Models_WhenCallModelWithNoLazyDetailProperty_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.Models<Region>().FirstOrDefault(x => x.Id == new Guid("90EDC2EE-07DD-DF11-971B-001D60E938C6"));

			// Assert
			Assert.IsNotNull(model);
			Assert.IsNotNull(model.Cities);
		}

		[Test, Order(1)]
		public void CaseInsert() {
			// Arrange
			var contact = _appDataContext.GetModel<Contact>(new Guid("6a910d79-bc71-46bc-9e58-89075f7395ba"));
			var model = _appDataContext.CreateModel<Lead>();
			model.ContactId = contact.Id;

			var trackerBeforeSave = _appDataContext.ChangeTracker.GetTrackedModel(model);
			Assert.IsNotNull(trackerBeforeSave);
			Assert.AreSame(model, trackerBeforeSave.Model);
			Assert.AreEqual(ModelState.New, trackerBeforeSave.GetStatus());

			// Act
			_appDataContext.Save();

			// Assert
			var trackerAfterSave = _appDataContext.ChangeTracker.GetTrackedModel(model);
			Assert.IsNotNull(trackerAfterSave);
			Assert.AreSame(model, trackerAfterSave.Model);
			Assert.AreEqual(ModelState.Unchanged, trackerAfterSave.GetStatus());
		}

		[Test, Order(2)]
		public void CaseUpdate() {
			// Arrange
			var model = _appDataContext.Models<Lead>()
				.FirstOrDefault(x => x.ContactId == new Guid("6a910d79-bc71-46bc-9e58-89075f7395ba"));

			// Act
			model.AnnualRevenueBC = 120m;
			_appDataContext.Save();

			// Assert
			Assert.IsNotNull(model);
		}

		[Test, Order(3)]
		public void CaseDelete() {
			// Arrange
			var model = _appDataContext.Models<Lead>()
				.FirstOrDefault(x => x.ContactId == new Guid("6a910d79-bc71-46bc-9e58-89075f7395ba"));

			var trackerBeforeSave = _appDataContext.ChangeTracker.GetTrackedModel(model);
			Assert.IsNotNull(trackerBeforeSave);
			Assert.AreSame(model, trackerBeforeSave.Model);
			Assert.AreEqual(ModelState.Unchanged, trackerBeforeSave.GetStatus());

			// Act
			_appDataContext.DeleteModel(model);
			_appDataContext.Save();

			// Assert
			var trackerAfterSave = _appDataContext.ChangeTracker.GetTrackedModel(model);
			Assert.IsNull(trackerAfterSave);
			Assert.IsTrue(model.IsMarkAsDeleted);
		}

		[Test, Order(4)]
		public void CaseInsertWithAllDataValueTypes() {
			// Arrange
			const decimal budget = 1100.15m;
			var title = "Test injected opportunity";
			var account = _appDataContext.Models<Account>().FirstOrDefault();
			var dueDate = new DateTime(2025, 5, 27);
			var isPrimary = true;
			var licenseCount = 110;

			// Act
			var accountType = _appDataContext.Models<AccountType>()
				.FirstOrDefault();
			var model = _appDataContext.CreateModel<Opportunity>();
			model.Title = title;
			model.Budget = budget;
			model.TypeId = accountType.Id;
			model.AccountId = account.Id;
			model.DueDate = dueDate;
			model.IsPrimary = isPrimary;
			model.LicenseCount = licenseCount;

			var trackerBeforeSave = _appDataContext.ChangeTracker.GetTrackedModel(model);
			Assert.IsNotNull(trackerBeforeSave);
			Assert.AreSame(model, trackerBeforeSave.Model);
			Assert.AreEqual(ModelState.New, trackerBeforeSave.GetStatus());


			var response = _appDataContext.Save();
			Assert.IsNotNull(response);
			Assert.IsTrue(response.Success);
			Assert.IsEmpty(response.ErrorMessage);

			// Assert
			var trackerAfterSave = _appDataContext.ChangeTracker.GetTrackedModel(model);
			Assert.IsNotNull(trackerAfterSave);
			Assert.AreSame(model, trackerAfterSave.Model);
			Assert.AreEqual(ModelState.Unchanged, trackerAfterSave.GetStatus());
			var savedModel = _secondaryAppDataContext.Models<Opportunity>().FirstOrDefault(x => x.Id == model.Id);
			Assert.IsNotNull(savedModel);
			Assert.AreEqual(model.Id, savedModel.Id);
			Assert.AreEqual(budget, savedModel.Budget);
			Assert.AreEqual(title, savedModel.Title);
			Assert.AreEqual(accountType.Id, savedModel.Type?.Id);
			Assert.AreEqual(account.Id, savedModel.AccountId);
			Assert.AreEqual(dueDate, savedModel.DueDate);
			Assert.AreEqual(isPrimary, savedModel.IsPrimary);
			Assert.AreEqual(licenseCount, savedModel.LicenseCount);
		}

		[Test, Order(5)]
		public void CaseUpdateWithAllDataValueTypes() {
			// Arrange
			var sponsorshipSaleTypeId = new Guid("4261485a-7bb4-4bbf-82ec-58df37fec1c9");
			const decimal budget = 1200.15m;
			var currentTitle = "Test injected opportunity";
			var newTitle = "Test injected opportunity1";
			var dueDate = new DateTime(2026, 5, 27);
			var licenseCount = 120;

			var model = _appDataContext.Models<Opportunity>().OrderByDescending(x=>x.CreatedOn).FirstOrDefault(x => x.Title.StartsWith(currentTitle));
			model.Budget = budget;
			model.Title = newTitle;
			
			var otherType = _appDataContext.Models<AccountType>()
				.FirstOrDefault(x => x.Id != model.TypeId);
			
			var otherAccount = _appDataContext.Models<Account>()
				.FirstOrDefault(x => x.Id != model.AccountId);
			
			model.TypeId = otherType.Id;
			model.AccountId = otherAccount.Id;
			model.DueDate = dueDate;
			model.LicenseCount = licenseCount;

			var trackerBeforeSave = _appDataContext.ChangeTracker.GetTrackedModel(model);
			Assert.IsNotNull(trackerBeforeSave);
			Assert.AreSame(model, trackerBeforeSave.Model);
			Assert.AreEqual(ModelState.Changed, trackerBeforeSave.GetStatus());

			// Act
			var response = _appDataContext.Save();

			// Assert
			Assert.IsTrue(response.Success);
			Assert.IsEmpty(response.ErrorMessage);

			var trackerAfterSave = _appDataContext.ChangeTracker.GetTrackedModel(model);
			Assert.IsNotNull(trackerAfterSave);
			Assert.AreSame(model, trackerAfterSave.Model);
			Assert.AreEqual(ModelState.Unchanged, trackerAfterSave.GetStatus());
			var savedModel = _secondaryAppDataContext.Models<Opportunity>().FirstOrDefault(x => x.Id == model.Id);
			Assert.IsNotNull(savedModel);
			Assert.AreEqual(model.Id, savedModel.Id);
			Assert.AreEqual(budget, savedModel.Budget);
			Assert.AreEqual(newTitle, savedModel.Title);
			Assert.AreEqual(otherType.Id, savedModel.Type?.Id);
			Assert.AreEqual(otherAccount.Id, savedModel.AccountId);
			Assert.AreEqual(dueDate, savedModel.DueDate);
			Assert.AreEqual(licenseCount, savedModel.LicenseCount);
		}

		[Test, Order(6)]
		public void CaseDeleteWithUndeletingLinkedData() {
			// Arrange
			var newTitle = "Test injected opportunity";
			var model = _appDataContext.Models<Opportunity>().OrderByDescending(x=>x.CreatedOn).FirstOrDefault(x => x.Title.StartsWith(newTitle));
			
			var contact = _appDataContext.Models<Contact>()
				.FirstOrDefault(x=>x.Name.Contains("Supervisor"));

			var detailModel = _appDataContext.CreateModel<OpportunityContact>();
			detailModel.OpportunityId = model.Id;
			detailModel.ContactId = contact.Id;
			_appDataContext.Save();
			
			_appDataContext.DeleteModel(model);

			var trackerBeforeSave = _appDataContext.ChangeTracker.GetTrackedModel(model);
			Assert.IsNotNull(trackerBeforeSave);
			Assert.AreSame(model, trackerBeforeSave.Model);
			Assert.AreEqual(ModelState.Deleted, trackerBeforeSave.GetStatus());

			// Act
			var response = _appDataContext.Save();
			Assert.IsFalse(response.Success);
		}

		[Test]
		public void Models_WhenCallModelWithDetailProperty_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.Models<Contact>().Where(x =>
				x.AccountId == new Guid("405947d0-2ffb-4ded-8675-0475f19f5a81") &&
				x.ContactInTags.Any(y => y.TagId == new Guid("55bd51a6-1abb-4e94-81ae-04cf92e49c41"))).ToList();

			// Assert
			Assert.IsNotNull(model);
			Assert.AreEqual(3, model.Count);
		}

		[Test]
		public void Models_WhenCallModelWithDetailSumProperty_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.Models<Account>().Where(x =>
				x.Id == new Guid("405947d0-2ffb-4ded-8675-0475f19f5a81") &&
				x.Contacts.Where(y=>y.ContactInTags.Any(z => z.TagId == new Guid("55bd51a6-1abb-4e94-81ae-04cf92e49c41"))).Sum(y=>y.Age) > 10 ).ToList();

			// Assert
			Assert.IsNotNull(model);
			Assert.AreEqual(1, model.Count);
		}

		[Test]
		public void Models_WhenCallModelWithDetailMaxProperty_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.Models<Account>().Where(x =>
				x.Id == new Guid("405947d0-2ffb-4ded-8675-0475f19f5a81") &&
				x.Contacts.Where(y=>y.ContactInTags.Any(z => z.TagId == new Guid("55bd51a6-1abb-4e94-81ae-04cf92e49c41"))).Max(y=>y.Age) > 10 ).ToList();

			// Assert
			Assert.IsNotNull(model);
			Assert.AreEqual(1, model.Count);
		}

		[Test]
		public void Models_WhenCallModelWithDetailPartMinProperty_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.Models<Account>().Where(x =>
				x.Id == new Guid("405947d0-2ffb-4ded-8675-0475f19f5a81") &&
				x.Contacts.Where(y=>y.ContactInTags.Any(z => z.TagId == new Guid("55bd51a6-1abb-4e94-81ae-04cf92e49c41")) && y.Age < 38).Max(y=>y.Age) < 38 ).ToList();

			// Assert
			Assert.IsNotNull(model);
			Assert.AreEqual(1, model.Count);
		}

		[Test]
		public void Models_WhenCallModelWity_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.Models<Account>().Where(x =>
				x.Id == new Guid("405947d0-2ffb-4ded-8675-0475f19f5a81") &&
				x.Contacts.Count(y => y.ContactInTags.Any(z => z.TagId == new Guid("55bd51a6-1abb-4e94-81ae-04cf92e49c41")) && y.Age < 38) == 1 ).ToList();

			// Assert
			Assert.IsNotNull(model);
			Assert.AreEqual(1, model.Count);
		}

		[Test]
		public void Models_WhenCallModelWithDetailPartMaxProperty_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.Models<Account>().Where(x =>
				x.Id == new Guid("405947d0-2ffb-4ded-8675-0475f19f5a81") &&
				x.Contacts.Where(y=>y.ContactInTags.Any(z => z.TagId == new Guid("55bd51a6-1abb-4e94-81ae-04cf92e49c41")) && y.Age > 38).Max(y=>y.Age) > 38 ).ToList();

			var models = _appDataContext.Models<Contact>().Where(x => x.Age > 10)
				.Where(x => x.TypeId == new Guid("55bd51a6-1abb-4e94-81ae-04cf92e49c41")).Average(x=>x.Age);

			// Assert
			Assert.IsNotNull(model);
			Assert.AreEqual(1, model.Count);
		}

		[Test]
		public void Models_WhenCallModelWithDatePartPartFilters_ShouldReturnsExpectedValue() {
			// Act
			var models = _appDataContext.Models<City>().Where(x =>
				x.CreatedOn.Hour == 12 &&
				x.CreatedOn.Year > 2019).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.IsTrue(models.All(x=>x.CreatedOn.Hour == 12 && x.CreatedOn.Year > 2019));
		}

		[Test]
		public void CreateModel_WhenHandleException_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.CreateModel<ActivityCategory>();
			model.Name = Guid.NewGuid().ToString();
			var response = _appDataContext.Save();

			// Assert
			Assert.IsFalse(response.Success);
			
		}

		[Test]
		public void SysSettings_WhenGetDateTimeValue_ShouldReturnsExpectedValue() {
			TestGetSysSettingsValue<DateTime>("CalculateClientARRFromDate",
				new DateTime(2025, 12, 4, 13, 15, 0));
		}

		[Test]
		public void SysSettings_WhenGetTimeValue_ShouldReturnsExpectedValue() {
			TestGetSysSettingsValue<DateTime>("AutomaticAgeActualizationTime",
				new DateTime(1900, 1, 1, 3, 30, 0));
		}

		[Test]
		public void SysSettings_WhenGetStringValue_ShouldReturnsExpectedValue() {
			TestGetSysSettingsValue<string>("FinishedTaskColor", "#A0A0A0");
		}

		[Test]
		public void SysSettings_WhenGetIntegerValue_ShouldReturnsExpectedValue() {
			TestGetSysSettingsValue<int>("MaxFileSize", 10);
		}

		[Test]
		public void SysSettings_WhenGetDecimalValue_ShouldReturnsExpectedValue() {
			TestGetSysSettingsValue<decimal>("SyncMemoryLimitToDeallocate", 100.00m);
		}

		[Test]
		public void SysSettings_WhenGetBooleanValue_ShouldReturnsExpectedValue() {
			TestGetSysSettingsValue<bool>("EnableRightsOnServiceObjects", false);
		}

		[Test]
		public void SysSettings_WhenGetLookupValue_ShouldReturnsExpectedValue() {
			TestGetSysSettingsValue<Guid>("PrimaryCurrency", new Guid("915e8a55-98d6-df11-9b2a-001d60e938c6"));
		}

		private void TestGetSysSettingsValue<T>(string code, T expectedValue) {
			// Act
			var response = _appDataContext.GetSysSettingValue<T>(code);

			// Assert
			Assert.IsNotNull(response);
			Assert.IsTrue(response.Success);
			Assert.IsNull(response.ErrorMessage);
			Assert.AreEqual(expectedValue, response.Value);
		}

		[Test]
		[TestCase("AddSecurityTypeToFilters", true)]
		[TestCase("AbortQueryOnDestroy-NotExisted", false)]
		[TestCase("AvalaraIntegrationEnabled", false)]
		public void GetFeatureEnabled_ShouldReturnsExpectedValue(string code, bool expectedValue) {
			var response = _appDataContext.GetFeatureEnabled(code);
			Assert.IsNotNull(response);
			Assert.IsTrue(response.Success);
			Assert.IsNull(response.ErrorMessage);
			Assert.AreEqual(expectedValue, response.Enabled);
		}
	}
}
