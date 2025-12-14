namespace ATF.Repository.UnitTests
{
	using System;
	using System.Linq;
	using ATF.Repository.Providers;
	using ATF.Repository.UnitTests.Models;
	using NUnit.Framework;

	[TestFixture]
	public class SelectSimpleTests : BaseIntegrationTests
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
		public void SimpleTest_Account_WithoutSelect()
		{
			// Test with Account entity using provided accountId
			var accountId = new Guid("405947d0-2ffb-4ded-8675-0475f19f5a81");

			var result = _appDataContext.Models<Account>()
				.Where(x => x.Id == accountId)
				.FirstOrDefault();

			Console.WriteLine($"Account found: {result != null}");
			if (result != null)
			{
				Console.WriteLine($"Account Id: {result.Id}");
				Console.WriteLine($"Account Name: {result.Name}");
			}

			Assert.IsNotNull(result, "Account should exist");
		}

		[Test]
		[Category("Integration")]
		public void SimpleTest_Account_WithSelect()
		{
			// Test with Select on Account
			var accountId = new Guid("405947d0-2ffb-4ded-8675-0475f19f5a81");

			var result = _appDataContext.Models<Account>()
				.Where(x => x.Id == accountId)
				.Select(x => new { x.Name })
				.FirstOrDefault();

			Console.WriteLine($"Account result: {result != null}");
			if (result != null)
			{
				Console.WriteLine($"Account Name: {result.Name}");
			}

			Assert.IsNotNull(result, "Account with Select should return result");
			Assert.IsNotNull(result.Name, "Account Name should not be null");
		}

		[Test]
		[Category("Integration")]
		public void SimpleTest_Contact_Top5_WithoutSelect()
		{
			// Get any 5 contacts
			var results = _appDataContext.Models<Contact>()
				.Take(5)
				.ToList();

			Console.WriteLine($"Contacts found: {results.Count}");
			Assert.Greater(results.Count, 0, "Should have at least some contacts");

			if (results.Count > 0)
			{
				Console.WriteLine($"First contact: {results.First().Name}");
			}
		}

		[Test]
		[Category("Integration")]
		public void SimpleTest_Contact_Top5_WithSelect()
		{
			// Get any 5 contacts with Select
			var results = _appDataContext.Models<Contact>()
				.Select(x => new { x.Name })
				.Take(5)
				.ToList();

			Console.WriteLine($"Contacts with Select found: {results.Count}");

			if (results.Count > 0)
			{
				Console.WriteLine($"First contact name: {results.First().Name}");
				Assert.IsNotNull(results.First().Name);
			}
			else
			{
				Assert.Fail("Should have returned at least one contact");
			}
		}

		[Test]
		[Category("Integration")]
		public void SimpleTest_Account_SelectSingleProperty()
		{
			// Test selecting a single property (not anonymous object)
			var accountId = new Guid("405947d0-2ffb-4ded-8675-0475f19f5a81");

			var result = _appDataContext.Models<Account>()
				.Where(x => x.Id == accountId)
				.Select(x => x.Name)
				.FirstOrDefault();

			Console.WriteLine($"Account Name result: {result}");
			Assert.IsNotNull(result, "Account Name should not be null");
		}

		[Test]
		[Category("Integration")]
		public void SimpleTest_Contact_Top5_SelectSingleProperty()
		{
			// Test selecting a single property from multiple records
			var results = _appDataContext.Models<Contact>()
				.Select(x => x.Name)
				.Take(5)
				.ToList();

			Console.WriteLine($"Contact names found: {results.Count}");
			Assert.Greater(results.Count, 0, "Should have at least some contact names");

			if (results.Count > 0)
			{
				Console.WriteLine($"First contact name: {results.First()}");
				Assert.IsNotNull(results.First());
			}
		}
	}
}
