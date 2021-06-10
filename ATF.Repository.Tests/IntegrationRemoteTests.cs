using System.Collections.Generic;

namespace ATF.Repository.Tests
{
	using System;
	using System.Linq;
	using ATF.Repository.Providers;
	using ATF.Repository.Tests.Models;
	using NUnit.Framework;

	[TestFixture]
	public class IntegrationRemoteTests
	{
		private RemoteDataProvider _remoteDataProvider;
		private IAppDataContext _appDataContext;

		[OneTimeSetUp]
		public void OneTimeSetUp() {
			_remoteDataProvider = new RemoteDataProvider("", "", "");
			_appDataContext = AppDataContextFactory.GetAppDataContext(_remoteDataProvider);
		}

		[Test]
		public void Models_WhenCallWithoutAnyFilters_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<AccountType>().ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(9, models.Count);
			Assert.IsTrue(models.All(x=>x.Id != Guid.Empty));
			Assert.IsTrue(models.All(x=>!string.IsNullOrEmpty(x.Name)));
		}

		[Test]
		public void Models_WhenCallWithoutAnyFilters_ShouldReturnsLessOrEqualMaxCountModels() {
			// Act
			var models = _appDataContext.Models<City>().ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.LessOrEqual(models.Count, 100);
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
			Assert.AreEqual("Supervisor", models.First().Name);
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
			var models = _appDataContext.Models<Contact>().Where(x=> x.Name.StartsWith("Superv")).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual("Supervisor", models.First().Name);
		}

		[Test]
		public void Models_WhenUseStringEndWithFilter_ShouldReturnExpectedValue() {
			// Act
			var models = _appDataContext.Models<Contact>().Where(x=> x.Name.EndsWith("rvisor")).Take(1).ToList();

			// Assert
			Assert.IsNotNull(models);
			Assert.AreEqual(1, models.Count);
			Assert.AreNotEqual(Guid.Empty, models.First().Id);
			Assert.AreEqual("Supervisor", models.First().Name);
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
				x.Name == "Supervisor" && x.TypeId == new Guid("60733efc-f36b-1410-a883-16d83cab0980"));

			// Assert
			Assert.IsNotNull(model);
			Assert.AreEqual("Supervisor", model.Name);
			Assert.AreEqual(new Guid("60733efc-f36b-1410-a883-16d83cab0980"), model.TypeId);
		}

		[Test]
		public void Models_WhenUseContainsStringFilter_ShouldReturnExpectedValue() {
			// Arrange
			var names = new List<string>() {"Supervisor", "Manager"};

			// Act
			var models = _appDataContext.Models<Contact>().Where(x => names.Contains(x.Name)).ToList();

			// Assert
			Assert.IsNotNull(models);
			models.ForEach(x => {
				Assert.Contains(x.Name, names);
			});
		}

		[Test]
		public void Models_WhenUseNotContainsStringFilter_ShouldReturnExpectedValue() {
			// Arrange
			var names = new List<string>() {"Supervisor", "Manager"};

			// Act
			var models = _appDataContext.Models<Contact>().Where(x => !names.Contains(x.Name)).Take(10).ToList();

			// Assert
			Assert.IsNotNull(models);
			models.ForEach(x => {
				Assert.IsTrue(!names.Contains(x.Name));
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
			var models = _appDataContext.Models<Contact>().Where(x => !types.Contains(x.TypeId)).ToList();

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
			var model = _appDataContext.Models<Region>().FirstOrDefault(x => x.Id == new Guid("d8bf2e4c-f36b-1410-fd98-00155d043204"));

			// Assert
			Assert.IsNotNull(model);
			Assert.IsNotNull(model.Country);
		}

		[Test]
		public void Models_WhenCallModelWithNoLazyDetailProperty_ShouldReturnsExpectedValue() {
			// Act
			var model = _appDataContext.Models<Region>().FirstOrDefault(x => x.Id == new Guid("d8bf2e4c-f36b-1410-fd98-00155d043204"));

			// Assert
			Assert.IsNotNull(model);
			Assert.IsNotNull(model.Cities);
		}

		[Test]
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

		[Test]
		public void CaseInsert() {
			// Arrange
			var model = _appDataContext.CreateModel<PainChain>();
			model.LeadId = new Guid("e579254e-6061-4b0e-b3f8-5c421e3283b2");
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

		[Test]
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
	}
}
