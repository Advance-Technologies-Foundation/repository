﻿namespace ATF.Repository.UnitTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.Providers;
	using ATF.Repository.UnitTests.Models;
	using NUnit.Framework;

	[TestFixture]
	public class IntegrationRemoteTests
	{
		private RemoteDataProvider _remoteDataProvider;
		private IAppDataContext _appDataContext;
		private IAppDataContext _secondaryAppDataContext;

		[OneTimeSetUp]
		public void OneTimeSetUp() {
			_remoteDataProvider = new RemoteDataProvider("", "", "");
		}

		[SetUp]
		public void SetUp() {
			_appDataContext = AppDataContextFactory.GetAppDataContext(_remoteDataProvider);
			_secondaryAppDataContext = AppDataContextFactory.GetAppDataContext(_remoteDataProvider);
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
			var models = _appDataContext.Models<Account>().Where(x=> x.ExactNoOfEmployees == 0).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual(0, models.First().ExactNoOfEmployees);
		}

		[Test]
		public void Models_WhenUseIntegerNotEqualFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Account>().Where(x=> x.ExactNoOfEmployees != 0).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreNotEqual(0, models.First().ExactNoOfEmployees);
		}

		[Test]
		public void Models_WhenUseIntegerGreaterFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Account>().Where(x=> x.ExactNoOfEmployees > 0).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.Greater(models.First().ExactNoOfEmployees, 0);
		}

		[Test]
		public void Models_WhenUseIntegerGreaterOrEqualFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Account>().Where(x=> x.ExactNoOfEmployees >= 0).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.GreaterOrEqual(models.First().ExactNoOfEmployees, 0);
		}

		[Test]
		public void Models_WhenUseIntegerLessFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Account>().Where(x=> x.ExactNoOfEmployees < 10).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.Less(models.First().ExactNoOfEmployees, 10);
		}

		[Test]
		public void Models_WhenUseIntegerLessOrEqualFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Account>().Where(x=> x.ExactNoOfEmployees <= 10).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.LessOrEqual(models.First().ExactNoOfEmployees, 10);
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
			var leads = _appDataContext.Models<Lead>().Take(1).OrderByDescending(x => x.CreatedOn).ToList();
			var model = _appDataContext.CreateModel<PainChain>();
			model.LeadId = leads.First().Id;
			model.KeyPlayerId = new Guid("410006e1-ca4e-4502-a9ec-e54d922d2c00");

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
				.FirstOrDefault(x => x.Id == new Guid("e579254e-6061-4b0e-b3f8-5c421e3283b2"));

			// Act
			model.AnnualRevenueBC = 120m;
			_appDataContext.Save();

			// Assert
			Assert.IsNotNull(model);
		}

		[Test, Order(3)]
		public void CaseDelete() {
			// Arrange
			var model = _appDataContext.Models<PainChain>()
				.FirstOrDefault(x => x.LeadId == new Guid("e579254e-6061-4b0e-b3f8-5c421e3283b2"));

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
			var supervisorContactId = new Guid("410006e1-ca4e-4502-a9ec-e54d922d2c00");
			var contactPersonRoleId = new Guid("8e0af235-a2c5-47e0-a80a-beee1740f9c6");
			var ceoJobId = new Guid("34f48df9-56e6-df11-971b-001d60e938c6");
			var directSaleTypeId = new Guid("3c3865f2-ada4-480c-ac91-e2d39c5bbaf9");
			var kameliaAccountId = new Guid("95391265-756d-4b73-b410-e178a7870f4f");
			var dueDate = new DateTime(2025, 5, 27);
			var isPrimary = true;
			var licenseCount = 110;
			var easternEuropeTerritoryId = new Guid("70B0CACE-B827-4758-9C03-3A63AAB256C5");
			var closedOnDate = new DateTime(2025, 5, 28, 14, 15, 15);
			var bpmLeadTypeId = new Guid("066dda2c-29ac-4c4c-9ec9-ca1d2ad653f1");

			// Act
			var directSaleType = _appDataContext.Models<OpportunityType>()
				.FirstOrDefault(x => x.Id == directSaleTypeId);
			var model = _appDataContext.CreateModel<Opportunity>();
			model.Budget = budget;
			model.Title = title;
			model.Type = directSaleType;
			model.AccountId = kameliaAccountId;
			model.DueDate = dueDate;
			model.IsPrimary = isPrimary;
			model.LicenseCount = licenseCount;
			model.TerritoryId = easternEuropeTerritoryId;
			model.ClosedOnDate = closedOnDate;
			model.LeadTypeId = bpmLeadTypeId;

			var trackerBeforeSave = _appDataContext.ChangeTracker.GetTrackedModel(model);
			Assert.IsNotNull(trackerBeforeSave);
			Assert.AreSame(model, trackerBeforeSave.Model);
			Assert.AreEqual(ModelState.New, trackerBeforeSave.GetStatus());


			var response = _appDataContext.Save();
			Assert.IsNotNull(response);
			Assert.IsTrue(response.Success);
			Assert.IsNull(response.ErrorMessage);

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
			Assert.AreEqual(directSaleTypeId, savedModel.Type?.Id);
			Assert.AreEqual(kameliaAccountId, savedModel.AccountId);
			Assert.AreEqual(dueDate, savedModel.DueDate);
			Assert.AreEqual(isPrimary, savedModel.IsPrimary);
			Assert.AreEqual(licenseCount, savedModel.LicenseCount);
			Assert.AreEqual(easternEuropeTerritoryId, savedModel.TerritoryId);
			Assert.AreEqual(closedOnDate, savedModel.ClosedOnDate);
			Assert.AreEqual(bpmLeadTypeId, savedModel.LeadTypeId);

			var contact = _appDataContext.Models<Contact>().FirstOrDefault(x => x.Id == supervisorContactId);
			var opportunityContact = _appDataContext.CreateModel<OpportunityContact>();
			opportunityContact.OpportunityId = savedModel.Id;
			opportunityContact.Contact = contact;
			opportunityContact.RoleId = contactPersonRoleId;
			opportunityContact.JobId = ceoJobId;
			_appDataContext.Save();

			Assert.AreEqual(1, savedModel.OpportunityContacts.Count());

		}

		[Test, Order(5)]
		public void CaseUpdateWithAllDataValueTypes() {
			// Arrange
			var sponsorshipSaleTypeId = new Guid("4261485a-7bb4-4bbf-82ec-58df37fec1c9");
			const decimal budget = 1200.15m;
			var currentTitle = "Test injected opportunity";
			var newTitle = "Test injected opportunity1";
			//var testContactId = new Guid("9f08f94a-be0a-457f-a30f-99fda3fb49dd");
			var directSaleTypeId = new Guid("3c3865f2-ada4-480c-ac91-e2d39c5bbaf9");
			var testAccountId = new Guid("7a6f2144-a972-423b-8cc4-08a68a48ddba");
			var dueDate = new DateTime(2026, 5, 27);
			//var isPrimary = false;
			var licenseCount = 120;
			var engTerritoryId = new Guid("70b0cace-b827-4758-9c03-3a63aab256c5");
			//var closedOnDate = new DateTime(2026, 5, 28, 14, 15, 15);
			var biLeadTypeId = new Guid("e40dd08d-612f-4864-acb3-d54e1957b7f6");

			var sponsorshipSaleType = _appDataContext.Models<OpportunityType>()
				.FirstOrDefault(x => x.Id == sponsorshipSaleTypeId);

			var model = _appDataContext.Models<Opportunity>().OrderByDescending(x=>x.CreatedOn).FirstOrDefault(x => x.Title.StartsWith(currentTitle));
			model.Budget = budget;
			model.Title = newTitle;
			model.Type = sponsorshipSaleType;
			model.AccountId = testAccountId;
			model.DueDate = dueDate;
			model.LicenseCount = licenseCount;
			model.TerritoryId = engTerritoryId;
			model.LeadTypeId = biLeadTypeId;

			var trackerBeforeSave = _appDataContext.ChangeTracker.GetTrackedModel(model);
			Assert.IsNotNull(trackerBeforeSave);
			Assert.AreSame(model, trackerBeforeSave.Model);
			Assert.AreEqual(ModelState.Changed, trackerBeforeSave.GetStatus());

			// Act
			var response = _appDataContext.Save();

			// Assert
			Assert.IsTrue(response.Success);
			Assert.IsNull(response.ErrorMessage);

			var trackerAfterSave = _appDataContext.ChangeTracker.GetTrackedModel(model);
			Assert.IsNotNull(trackerAfterSave);
			Assert.AreSame(model, trackerAfterSave.Model);
			Assert.AreEqual(ModelState.Unchanged, trackerAfterSave.GetStatus());
			var savedModel = _secondaryAppDataContext.Models<Opportunity>().FirstOrDefault(x => x.Id == model.Id);
			Assert.IsNotNull(savedModel);
			Assert.AreEqual(model.Id, savedModel.Id);
			Assert.AreEqual(budget, savedModel.Budget);
			Assert.AreEqual(newTitle, savedModel.Title);
			Assert.AreEqual(sponsorshipSaleTypeId, savedModel.Type?.Id);
			Assert.AreEqual(testAccountId, savedModel.AccountId);
			Assert.AreEqual(dueDate, savedModel.DueDate);
			Assert.AreEqual(licenseCount, savedModel.LicenseCount);
			Assert.AreEqual(engTerritoryId, savedModel.TerritoryId);
			Assert.AreEqual(biLeadTypeId, savedModel.LeadTypeId);
		}

		[Test, Order(6)]
		public void CaseDeleteWithUndeletingLinkedData() {
			// Arrange
			var newTitle = "Test injected opportunity";
			var model = _appDataContext.Models<Opportunity>().OrderByDescending(x=>x.CreatedOn).FirstOrDefault(x => x.Title.StartsWith(newTitle));
			_appDataContext.DeleteModel(model);

			var trackerBeforeSave = _appDataContext.ChangeTracker.GetTrackedModel(model);
			Assert.IsNotNull(trackerBeforeSave);
			Assert.AreSame(model, trackerBeforeSave.Model);
			Assert.AreEqual(ModelState.Deleted, trackerBeforeSave.GetStatus());

			// Act
			var response = _appDataContext.Save();
			Assert.IsFalse(response.Success);
		}

		[Test, Order(7)]
		public void CaseInsert_WhenUseProduct_ShouldReturnExpectedValue() {
			// Arrange
			var code = Guid.NewGuid().ToString().Substring(0, 6);
			var contact = _appDataContext.Models<Contact>().FirstOrDefault(x => x.Id == new Guid("410006e1-ca4e-4502-a9ec-e54d922d2c00"));
			var currency = _appDataContext.Models<Currency>().FirstOrDefault(x => x.Id == new Guid("915e8a55-98d6-df11-9b2a-001d60e938c6"));
			var productSource = _appDataContext.Models<ProductSource>().FirstOrDefault(x => x.Id == new Guid("5facb8b4-ed6a-41bb-b224-659c2bf1eb8a"));
			var productCategory = _appDataContext.Models<ProductCategory>().FirstOrDefault(x => x.Id == new Guid("ad0fecfd-3b90-45c4-a9ac-4af5e6bbd3f2"));
			var type = _appDataContext.Models<ProductType>().FirstOrDefault(x => x.Id == new Guid("64c3c3c1-706e-41c5-bebb-be04ae635bed"));
			var kind = _appDataContext.Models<ProductKind>().FirstOrDefault(x => x.Id == new Guid("d974deba-d49e-4306-9aa3-aca1fc5c8eb0"));

			// Act
			var product = _appDataContext.CreateModel<Product>();
			product.Name = "TestIntegrationProduct";
			product.Active = true;
			product.Owner = contact;
			product.UnitId = new Guid("b81f9062-62e6-df11-971b-001d60e938c6");
			product.Currency = currency;
			product.ProductSource = productSource;
			product.Price = 101.05m;
			product.IntegratedOn = DateTime.Now;
			product.StartDate = DateTime.Now;
			product.Code = code;
			product.Url = "URL";
			product.Category = productCategory;
			product.Type = type;
			product.IsTarget = false;
			product.Kind = kind;
			product.RevenueRecognitionMethodId = new Guid("6d954744-4af9-44e4-8ffd-ee8821485bf8");

			var trackerBeforeSave = _appDataContext.ChangeTracker.GetTrackedModel(product);
			Assert.IsNotNull(trackerBeforeSave);
			Assert.AreSame(product, trackerBeforeSave.Model);
			Assert.AreEqual(ModelState.New, trackerBeforeSave.GetStatus());

			var response = _appDataContext.Save();

			// Assert
			Assert.IsNotNull(response);
			Assert.IsFalse(response.Success);
		}

		[Test]
		public void Models_WhenCallModelWithDetailProperty_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.Models<Contact>().Where(x =>
				x.AccountId == new Guid("7a6f2144-a972-423b-8cc4-08a68a48ddba") &&
				x.ContactInTags.Any(y => y.TagId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef"))).ToList();

			// Assert
			Assert.IsNotNull(model);
			Assert.AreEqual(4, model.Count);
		}

		[Test]
		public void Models_WhenCallModelWithDetailSumProperty_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.Models<Account>().Where(x =>
				x.Id == new Guid("7a6f2144-a972-423b-8cc4-08a68a48ddba") &&
				x.Contacts.Where(y=>y.ContactInTags.Any(z => z.TagId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef"))).Sum(y=>y.Age) > 10 ).ToList();

			// Assert
			Assert.IsNotNull(model);
			Assert.AreEqual(1, model.Count);
		}

		[Test]
		public void Models_WhenCallModelWithDetailMaxProperty_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.Models<Account>().Where(x =>
				x.Id == new Guid("7a6f2144-a972-423b-8cc4-08a68a48ddba") &&
				x.Contacts.Where(y=>y.ContactInTags.Any(z => z.TagId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef"))).Max(y=>y.Age) > 10 ).ToList();

			// Assert
			Assert.IsNotNull(model);
			Assert.AreEqual(1, model.Count);
		}

		[Test]
		public void Models_WhenCallModelWithDetailPartMinProperty_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.Models<Account>().Where(x =>
				x.Id == new Guid("7a6f2144-a972-423b-8cc4-08a68a48ddba") &&
				x.Contacts.Where(y=>y.ContactInTags.Any(z => z.TagId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef")) && y.Age < 38).Max(y=>y.Age) < 38 ).ToList();

			// Assert
			Assert.IsNotNull(model);
			Assert.AreEqual(1, model.Count);
		}

		[Test]
		public void Models_WhenCallModelWithDetailCountProperty_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.Models<Account>().Where(x =>
				x.Id == new Guid("7a6f2144-a972-423b-8cc4-08a68a48ddba") &&
				x.Contacts.Count(y => y.ContactInTags.Any(z => z.TagId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef")) && y.Age < 38) == 2 ).ToList();

			// Assert
			Assert.IsNotNull(model);
			Assert.AreEqual(1, model.Count);
		}

		[Test]
		public void Models_WhenCallModelWithDetailPartMaxProperty_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.Models<Account>().Where(x =>
				x.Id == new Guid("7a6f2144-a972-423b-8cc4-08a68a48ddba") &&
				x.Contacts.Where(y=>y.ContactInTags.Any(z => z.TagId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef")) && y.Age > 38).Max(y=>y.Age) > 38 ).ToList();

			var models = _appDataContext.Models<Contact>().Where(x => x.Age > 10)
				.Where(x => x.TypeId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef")).Average(x=>x.Age);

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
				new DateTime(2010, 1, 1, 2, 0, 0));
		}

		[Test]
		public void SysSettings_WhenGetTimeValue_ShouldReturnsExpectedValue() {
			TestGetSysSettingsValue<DateTime>("AutomaticAgeActualizationTime",
				new DateTime(1900, 1, 1, 5, 30, 0));
		}

		[Test]
		public void SysSettings_WhenGetStringValue_ShouldReturnsExpectedValue() {
			TestGetSysSettingsValue<string>("FinishedTaskColor", "#A0A0A0");
		}

		[Test]
		public void SysSettings_WhenGetIntegerValue_ShouldReturnsExpectedValue() {
			TestGetSysSettingsValue<int>("MaxFileSize", 400);
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
		[TestCase("AbortQueryOnDestroy", true)]
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
