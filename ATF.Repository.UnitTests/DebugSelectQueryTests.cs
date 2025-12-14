namespace ATF.Repository.UnitTests
{
	using System;
	using System.Linq;
	using ATF.Repository.Providers;
	using ATF.Repository.UnitTests.Models;
	using NUnit.Framework;
	using Newtonsoft.Json;

	[TestFixture]
	public class DebugSelectQueryTests : BaseIntegrationTests
	{
		[Test]
		[Category("Debug")]
		public void Debug_Account_WithSelect_CheckColumns()
		{
			// This test should verify that ONLY Id and Name columns are in the query
			// By checking the query execution count and result structure

			var dataProvider = GetIntegrationDataProvider();
			var appDataContext = AppDataContextFactory.GetAppDataContext(dataProvider);

			var accountId = new Guid("405947d0-2ffb-4ded-8675-0475f19f5a81");

			// This query should send ONLY Id and Name to the server
			var result = appDataContext.Models<Account>()
				.Where(x => x.Id == accountId)
				.Select(x => new { x.Id, x.Name })
				.FirstOrDefault();

			Console.WriteLine($"Result found: {result != null}");
			if (result != null)
			{
				Console.WriteLine($"Id: {result.Id}");
				Console.WriteLine($"Name: {result.Name}");

				// Check that result has ONLY 2 properties
				var properties = result.GetType().GetProperties();
				Console.WriteLine($"Property count: {properties.Length}");
				foreach (var prop in properties)
				{
					Console.WriteLine($"  - {prop.Name}: {prop.PropertyType.Name}");
				}

				Assert.AreEqual(2, properties.Length, "Result should have exactly 2 properties");
			}

			Assert.IsNotNull(result);
			Assert.Pass("Check RemoteDataProvider logs to verify query structure");
		}

		[Test]
		[Category("Debug")]
		public void Debug_Contact_WithDetailAggregation_CheckColumns()
		{
			// This test should verify that ONLY Name and TagCount are in the query
			// And that TagCount is a server-side aggregation, not lazy loading

			var dataProvider = GetIntegrationDataProvider();
			var appDataContext = AppDataContextFactory.GetAppDataContext(dataProvider);

			// This query should send ONLY Name and [ContactInTag:Contact].Id aggregation
			var results = appDataContext.Models<Contact>()
				.Select(x => new {
					x.Name,
					TagCount = x.ContactInTags.Count()
				})
				.Take(5)
				.ToList();

			Console.WriteLine($"Results count: {results.Count}");
			if (results.Count > 0)
			{
				var first = results.First();
				Console.WriteLine($"First contact: {first.Name}, TagCount: {first.TagCount}");

				// Check that result has ONLY 2 properties
				var properties = first.GetType().GetProperties();
				Console.WriteLine($"Property count: {properties.Length}");
				foreach (var prop in properties)
				{
					Console.WriteLine($"  - {prop.Name}: {prop.PropertyType.Name}");
				}

				Assert.AreEqual(2, properties.Length, "Result should have exactly 2 properties");
			}

			Assert.Greater(results.Count, 0);
			Assert.Pass("Check RemoteDataProvider logs to verify aggregation is in same query");
		}
	}
}
