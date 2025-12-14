namespace ATF.Repository.UnitTests
{
	using System;
	using System.Linq;
	using ATF.Repository.Providers;
	using ATF.Repository.UnitTests.Models;
	using NUnit.Framework;

	[TestFixture]
	public class SelectContactTests : BaseIntegrationTests
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
		public void Contact_Top10_WithSelect_SingleField()
		{
			var results = _appDataContext.Models<Contact>()
				.Select(x => new { x.Name })
				.Take(10)
				.ToList();

			Console.WriteLine($"Contacts with Select (Name only): {results.Count}");
			Assert.Greater(results.Count, 0, "Should have contacts");

			if (results.Count > 0)
			{
				var first = results.First();
				Console.WriteLine($"First contact: {first.Name}");
				Assert.IsNotNull(first.Name);
			}
		}

		[Test]
		[Category("Integration")]
		[Category("Select")]
		public void Contact_Top10_WithSelect_MultipleFields()
		{
			var results = _appDataContext.Models<Contact>()
				.Select(x => new { x.Id, x.Name, x.Email })
				.Take(10)
				.ToList();

			Console.WriteLine($"Contacts with Select (Id, Name, Email): {results.Count}");
			Assert.Greater(results.Count, 0);

			if (results.Count > 0)
			{
				var first = results.First();
				Console.WriteLine($"Id: {first.Id}, Name: {first.Name}, Email: {first.Email}");
				Assert.AreNotEqual(Guid.Empty, first.Id);
			}
		}

		[Test]
		[Category("Integration")]
		[Category("Select")]
		public void Contact_WithLookup_AccountName()
		{
			// Get contacts and include Account lookup
			var results = _appDataContext.Models<Contact>()
				.Select(x => new {
					x.Name,
					AccountName = x.Account.Name
				})
				.Take(10)
				.ToList();

			Console.WriteLine($"Contacts with Account lookup: {results.Count}");
			Assert.Greater(results.Count, 0);

			if (results.Count > 0)
			{
				var first = results.First();
				Console.WriteLine($"Contact: {first.Name}, Account: {first.AccountName ?? "(null)"}");

				// Check structure
				var properties = first.GetType().GetProperties();
				Assert.AreEqual(2, properties.Length);
				Assert.IsTrue(properties.Any(p => p.Name == "Name"));
				Assert.IsTrue(properties.Any(p => p.Name == "AccountName"));
			}
		}
	}
}
