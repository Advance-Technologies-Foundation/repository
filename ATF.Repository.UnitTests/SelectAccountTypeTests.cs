namespace ATF.Repository.UnitTests
{
	using System;
	using System.Linq;
	using ATF.Repository.Providers;
	using ATF.Repository.UnitTests.Models;
	using NUnit.Framework;

	[TestFixture]
	public class SelectAccountTypeTests : BaseIntegrationTests
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
		public void AccountType_WithoutSelect_ShouldReturnData()
		{
			var results = _appDataContext.Models<AccountType>().ToList();

			Console.WriteLine($"AccountTypes WITHOUT Select: {results.Count}");
			Assert.Greater(results.Count, 0);
			Assert.IsTrue(results.All(x => x.Id != Guid.Empty));
			Assert.IsTrue(results.All(x => !string.IsNullOrEmpty(x.Name)));
		}

		[Test]
		[Category("Integration")]
		[Category("Select")]
		public void AccountType_WithSelect_SingleField_ShouldReturnOnlyName()
		{
			var results = _appDataContext.Models<AccountType>()
				.Select(x => new { x.Name })
				.ToList();

			Console.WriteLine($"AccountTypes WITH Select (Name only): {results.Count}");

			Assert.Greater(results.Count, 0, "Should have AccountTypes");

			var first = results.First();
			Assert.IsNotNull(first);
			Assert.IsNotNull(first.Name);

			// Verify structure - should have only 1 property
			var properties = first.GetType().GetProperties();
			Assert.AreEqual(1, properties.Length, "Should have only Name property");
			Assert.AreEqual("Name", properties[0].Name);

			Console.WriteLine($"First AccountType Name: {first.Name}");
		}

		[Test]
		[Category("Integration")]
		[Category("Select")]
		public void AccountType_WithSelect_MultipleFields_ShouldReturnBothFields()
		{
			var results = _appDataContext.Models<AccountType>()
				.Select(x => new { x.Id, x.Name })
				.ToList();

			Console.WriteLine($"AccountTypes WITH Select (Id, Name): {results.Count}");

			Assert.Greater(results.Count, 0);

			var first = results.First();
			Assert.AreNotEqual(Guid.Empty, first.Id);
			Assert.IsNotNull(first.Name);

			// Verify structure
			var properties = first.GetType().GetProperties();
			Assert.AreEqual(2, properties.Length);

			Console.WriteLine($"First: Id={first.Id}, Name={first.Name}");
		}
	}
}
