namespace ATF.Repository.UnitTests
{
	using System;
	using System.Linq;
	using ATF.Repository.Providers;
	using ATF.Repository.UnitTests.Models;
	using NUnit.Framework;

	[TestFixture]
	public class SelectAccountTest : BaseIntegrationTests
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
		public void Account_ById_WithoutSelect_ShouldWork()
		{
			var accountId = new Guid("405947d0-2ffb-4ded-8675-0475f19f5a81");

			var result = _appDataContext.Models<Account>()
				.Where(x => x.Id == accountId)
				.FirstOrDefault();

			Console.WriteLine($"Account found: {result != null}");
			if (result != null)
			{
				Console.WriteLine($"Id: {result.Id}");
				Console.WriteLine($"Name: {result.Name}");
			}

			Assert.IsNotNull(result, "Account should exist");
			Assert.AreEqual(accountId, result.Id);
		}

		[Test]
		[Category("Integration")]
		public void Account_ById_WithSelect_ShouldWork()
		{
			var accountId = new Guid("405947d0-2ffb-4ded-8675-0475f19f5a81");

			var result = _appDataContext.Models<Account>()
				.Where(x => x.Id == accountId)
				.Select(x => new { x.Id, x.Name })
				.FirstOrDefault();

			Console.WriteLine($"Account with Select found: {result != null}");
			if (result != null)
			{
				Console.WriteLine($"Id: {result.Id}");
				Console.WriteLine($"Name: {result.Name}");
			}

			Assert.IsNotNull(result, "Account with Select should return result");
			Assert.AreEqual(accountId, result.Id);
			Assert.IsNotNull(result.Name);
		}

		[Test]
		[Category("Integration")]
		public void Account_Top10_WithoutSelect()
		{
			var results = _appDataContext.Models<Account>()
				.Take(10)
				.ToList();

			Console.WriteLine($"Accounts without Select: {results.Count}");
			Assert.Greater(results.Count, 0, "Should have accounts");
		}

		[Test]
		[Category("Integration")]
		public void Account_Top10_WithSelect()
		{
			var results = _appDataContext.Models<Account>()
				.Select(x => new { x.Name })
				.Take(10)
				.ToList();

			Console.WriteLine($"Accounts with Select: {results.Count}");

			if (results.Count > 0)
			{
				Console.WriteLine($"First account: {results.First().Name}");
			}

			Assert.Greater(results.Count, 0, "Should have accounts with Select");
		}
	}
}
