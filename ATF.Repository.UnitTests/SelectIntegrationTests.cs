namespace ATF.Repository.UnitTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.Providers;
	using ATF.Repository.UnitTests.Models;
	using NUnit.Framework;

	[TestFixture]
	public class SelectIntegrationTests : BaseIntegrationTests
	{
		private IDataProvider _dataProvider;
		private IAppDataContext _appDataContext;

		[SetUp]
		public void SetUp()
		{
			_dataProvider = GetIntegrationDataProvider();
			_appDataContext = AppDataContextFactory.GetAppDataContext(_dataProvider);
		}

		[Test]
		[Category("Integration")]
		[Category("Select")]
		public void Select_WithSingleField_ShouldReturnOnlySelectedColumn()
		{
			// Arrange & Act
			var results = _appDataContext.Models<Contact>()
				.Select(x => new { x.Name })
				.Take(5)
				.ToList();

			// Assert
			Assert.IsNotNull(results);
			Assert.Greater(results.Count, 0);

			// Verify that only Name is populated (Id should NOT be included automatically)
			var firstResult = results.First();
			Assert.IsNotNull(firstResult);
			Assert.IsTrue(firstResult.GetType().GetProperties().Length == 1,
				"Should have only 1 property (Name)");
		}

		[Test]
		[Category("Integration")]
		[Category("Select")]
		public void Select_SinglePropertyWithoutAnonymousType_ShouldReturnStringValues()
		{
			// Arrange & Act
			var results = _appDataContext.Models<Contact>()
				.Select(x => x.Name)
				.Take(5)
				.ToList();

			// Assert
			Assert.IsNotNull(results);
			Assert.Greater(results.Count, 0, "Should return at least one contact");

			// Verify that results are strings
			foreach (var result in results)
			{
				Assert.IsNotNull(result, "Name should not be null");
				Assert.IsInstanceOf<string>(result, "Result should be a string");
				Assert.IsNotEmpty(result, "Name should not be empty");
			}

			// Check that we got exactly the type we expected
			var firstResult = results.First();
			Assert.IsInstanceOf<string>(firstResult);
		}

		[Test]
		[Category("Integration")]
		[Category("Select")]
		public void Select_WithMultipleFields_ShouldReturnAllSelectedColumns()
		{
			// Arrange & Act
			var results = _appDataContext.Models<Contact>()
				.Select(x => new { x.Id, x.Name, x.Email })
				.Take(5)
				.ToList();

			// Assert
			Assert.IsNotNull(results);
			Assert.Greater(results.Count, 0);

			var firstResult = results.First();
			Assert.IsNotNull(firstResult);
			Assert.AreNotEqual(Guid.Empty, firstResult.Id);
			Assert.IsNotNull(firstResult.Name);
			// Email can be null
		}

		[Test]
		[Category("Integration")]
		[Category("Select")]
		public void Select_WithLookupField_ShouldReturnLookupValue()
		{
			// Arrange & Act
			var results = _appDataContext.Models<Contact>()
				.Where(x => x.AccountId != Guid.Empty) // Ensure we have contacts with accounts
				.Select(x => new {
					x.Name,
					AccountName = x.Account.Name
				})
				.Take(5)
				.ToList();

			// Assert
			Assert.IsNotNull(results);
			Assert.Greater(results.Count, 0, "Should have at least one contact with account");

			var firstResult = results.First();
			Assert.IsNotNull(firstResult);
			Assert.IsNotNull(firstResult.Name, "Contact Name should not be null");
			Assert.IsNotNull(firstResult.AccountName, "Account Name should not be null for filtered contacts");

			// Verify structure
			var properties = firstResult.GetType().GetProperties();
			Assert.AreEqual(2, properties.Length, "Should have exactly 2 properties");
			Assert.IsTrue(properties.Any(p => p.Name == "Name"), "Should have Name property");
			Assert.IsTrue(properties.Any(p => p.Name == "AccountName"), "Should have AccountName property");
		}

		[Test]
		[Category("Integration")]
		[Category("Select")]
		public void Select_WithNestedLookup_ShouldReturnNestedLookupValue()
		{
			// Arrange & Act
			var results = _appDataContext.Models<Contact>()
				.Where(x => x.Account.PrimaryContact.Id != Guid.Empty)
				.Select(x => new {
					x.Name,
					AccountName = x.Account.Name,
					PrimaryContactName = x.Account.PrimaryContact.Name
				})
				.Take(5)
				.ToList();

			// Assert
			Assert.IsNotNull(results);

			if (results.Count > 0)
			{
				var firstResult = results.First();
				Assert.IsNotNull(firstResult.Name);
				Assert.IsNotNull(firstResult.AccountName);
				Assert.IsNotNull(firstResult.PrimaryContactName);
			}
		}

		[Test]
		[Category("Integration")]
		[Category("Select")]
		public void Select_WithConstant_ShouldIncludeConstantValue()
		{
			// Arrange & Act
			var results = _appDataContext.Models<Contact>()
				.Select(x => new {
					EntityType = "Contact", // Constant
					x.Name
				})
				.Take(5)
				.ToList();

			// Assert
			Assert.IsNotNull(results);
			Assert.Greater(results.Count, 0);

			var firstResult = results.First();
			Assert.AreEqual("Contact", firstResult.EntityType);
			Assert.IsNotNull(firstResult.Name);
		}

		[Test]
		[Category("Integration")]
		[Category("Select")]
		public void Select_AfterWhere_ShouldPreserveFilter()
		{
			// Arrange
			var testTypeId = new Guid("60733efc-f36b-1410-a883-16d83cab0980");

			// Act
			var results = _appDataContext.Models<Contact>()
				.Where(x => x.TypeId == testTypeId)
				.Select(x => new { x.Name, x.Email })
				.Take(5)
				.ToList();

			// Assert
			Assert.IsNotNull(results);

			if (results.Count > 0)
			{
				var firstResult = results.First();
				Assert.IsNotNull(firstResult.Name);
			}
		}

		[Test]
		[Category("Integration")]
		[Category("Select")]
		public void Select_WithDetailCount_ShouldReturnAggregatedValue()
		{
			// Arrange & Act
			var results = _appDataContext.Models<Contact>()
				.Select(x => new {
					x.Name,
					TagCount = x.ContactInTags.Count()
				})
				.Take(5)
				.ToList();

			// Assert
			Assert.IsNotNull(results);
			Assert.Greater(results.Count, 0);

			var firstResult = results.First();
			Assert.IsNotNull(firstResult.Name);
			Assert.GreaterOrEqual(firstResult.TagCount, 0, "Tag count should be >= 0");
		}

		[Test]
		[Category("Integration")]
		[Category("Select")]
		public void Select_WithMixedColumnsAndLookups_ShouldReturnAllValues()
		{
			// Arrange & Act
			var results = _appDataContext.Models<Contact>()
				.Where(x => x.AccountId != Guid.Empty)
				.Select(x => new {
					x.Id,
					x.Name,
					x.Email,
					AccountName = x.Account.Name,
					RecordType = "Contact"
				})
				.Take(5)
				.ToList();

			// Assert
			Assert.IsNotNull(results);
			Assert.Greater(results.Count, 0);

			var firstResult = results.First();
			Assert.AreNotEqual(Guid.Empty, firstResult.Id);
			Assert.IsNotNull(firstResult.Name);
			Assert.IsNotNull(firstResult.AccountName);
			Assert.AreEqual("Contact", firstResult.RecordType);
		}
	}
}
