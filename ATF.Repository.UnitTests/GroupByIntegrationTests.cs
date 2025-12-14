namespace ATF.Repository.UnitTests
{
	using System;
	using System.Linq;
	using ATF.Repository.Providers;
	using ATF.Repository.Replicas;
	using ATF.Repository.UnitTests.Models;
	using Newtonsoft.Json;
	using NSubstitute;
	using NUnit.Framework;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;
	using FunctionType = Terrasoft.Nui.ServiceModel.DataContract.FunctionType;
	using QuerySource = Terrasoft.Nui.ServiceModel.DataContract.QuerySource;

	[TestFixture]
	public class GroupByIntegrationTests : BaseIntegrationTests
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
		[Category("GroupBy")]
		public void GroupBy_ContactsByAccount_ShouldReturnAccountNameAndContactCount()
		{
			// Arrange & Act - Group contacts by Account, return Account and Contact Count
			var results = _appDataContext.Models<Contact>()
				.Where(x => x.AccountId != Guid.Empty) // Only contacts with accounts
				.Take(10)
				.GroupBy(x => new { x.Account }, (groupBy, items) => new
				{
					groupBy.Account,
					ContactCount = items.Count()
				})
				.ToList();

			// Assert
			Assert.IsNotNull(results, "Results should not be null");
			Assert.Greater(results.Count, 0, "Should have at least one group (account with contacts)");

			Console.WriteLine($"Found {results.Count} accounts with contacts");

			// Check first result
			var firstResult = results.First();
			Assert.IsNotNull(firstResult, "First result should not be null");
			Assert.IsNotNull(firstResult.Account, "Account should not be null");
			Assert.Greater(firstResult.ContactCount, 0, "Contact count should be greater than 0");

			Console.WriteLine($"First group: Account ID '{firstResult.Account.Id}' has {firstResult.ContactCount} contact(s)");

			// Verify all results have valid data
			foreach (var result in results.Take(10))
			{
				Assert.IsNotNull(result.Account);
				Assert.AreNotEqual(Guid.Empty, result.Account.Id);
				Assert.Greater(result.ContactCount, 0);
				Console.WriteLine($"  - Account ID '{result.Account.Id}': {result.ContactCount} contact(s)");
			}
		}

		[Test]
		[Category("Integration")]
		[Category("GroupBy")]
		public void GroupBy_ContactsByAccountWithMultipleAggregations_ShouldReturnCorrectData()
		{
			// Arrange & Act - Group contacts by Account with multiple aggregations
			var results = _appDataContext.Models<Contact>()
				.Where(x => x.AccountId != Guid.Empty && x.Age > 0)
				.GroupBy(x => new { x.Account }, (groupBy, items) => new
				{
					groupBy.Account,
					ContactCount = items.Count(),
					TotalAge = items.Sum(c => c.Age),
					MaxAge = items.Max(c => c.Age),
					MinAge = items.Min(c => c.Age),
					AvgAge = items.Average(c => c.Age)
				})
				.ToList();

			// Assert
			Assert.IsNotNull(results, "Results should not be null");

			if (results.Count > 0)
			{
				Console.WriteLine($"Found {results.Count} accounts with contacts having age data");

				var firstResult = results.First();
				Assert.IsNotNull(firstResult.Account, "Account should not be null");
				Assert.Greater(firstResult.ContactCount, 0, "Contact count should be greater than 0");
				Assert.Greater(firstResult.TotalAge, 0, "Total age should be greater than 0");
				Assert.Greater(firstResult.MaxAge, 0, "Max age should be greater than 0");
				Assert.Greater(firstResult.MinAge, 0, "Min age should be greater than 0");
				Assert.Greater(firstResult.AvgAge, 0, "Average age should be greater than 0");

				Console.WriteLine($"Account ID '{firstResult.Account.Id}':");
				Console.WriteLine($"  - Contacts: {firstResult.ContactCount}");
				Console.WriteLine($"  - Total Age: {firstResult.TotalAge}");
				Console.WriteLine($"  - Max Age: {firstResult.MaxAge}");
				Console.WriteLine($"  - Min Age: {firstResult.MinAge}");
				Console.WriteLine($"  - Avg Age: {firstResult.AvgAge:F2}");
			}
			else
			{
				Assert.Inconclusive("No contacts with age data found in test environment");
			}
		}

		[Test]
		[Category("Integration")]
		[Category("GroupBy")]
		public void GroupBy_ContactsByMultipleKeys_ShouldReturnCorrectGrouping()
		{
			// Arrange & Act - Group by multiple keys (Account and Type)
			var results = _appDataContext.Models<Contact>()
				.Where(x => x.AccountId != Guid.Empty && x.TypeId != Guid.Empty)
				.GroupBy(x => new { x.Account, x.TypeId }, (groupBy, items) => new
				{
					groupBy.Account,
					groupBy.TypeId,
					ContactCount = items.Count()
				})
				.ToList();

			// Assert
			Assert.IsNotNull(results, "Results should not be null");

			if (results.Count > 0)
			{
				Console.WriteLine($"Found {results.Count} groups (Account + Type combinations)");

				var firstResult = results.First();
				Assert.IsNotNull(firstResult.Account, "Account should not be null");
				Assert.AreNotEqual(Guid.Empty, firstResult.TypeId, "Type Id should not be empty");
				Assert.Greater(firstResult.ContactCount, 0, "Contact count should be greater than 0");

				// Display first few groups
				foreach (var result in results.Take(5))
				{
					Console.WriteLine($"  - Account ID '{result.Account.Id}', Type: {result.TypeId}: {result.ContactCount} contact(s)");
				}
			}
			else
			{
				Assert.Inconclusive("No contacts with both Account and Type found in test environment");
			}
		}

		[Test]
		[Category("Unit")]
		[Category("GroupBy")]
		public void GroupBy_DebugJsonStructure_ShouldMatchCase2Txt()
		{
			// This test captures the actual JSON query for debugging purposes
			SelectQueryReplica actualQuery = null;

			// Create a mock provider to capture the query
			var mockProvider = Substitute.For<IDataProvider>();
			mockProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			var mockContext = AppDataContextFactory.GetAppDataContext(mockProvider);

			// Execute GroupBy query - matching case2.txt structure
			var results = mockContext.Models<Contact>()
				.Where(x => x.AccountId != Guid.Empty)
				.GroupBy(x => new { x.Account }, (groupBy, items) => new
				{
					groupBy.Account,
					ContactCount = items.Count()
				})
				.ToList();

			// Assert and print JSON
			Assert.IsNotNull(actualQuery, "Query should be captured");

			var json = JsonConvert.SerializeObject(actualQuery, Formatting.Indented);
			Console.WriteLine("=== Generated GroupBy Query JSON ===");
			Console.WriteLine(json);
			Console.WriteLine("====================================");

			// Verify key properties match case2.txt structure
			Assert.AreEqual(false, actualQuery.IsPageable, "IsPageable should be false");
			Assert.AreEqual(-1, actualQuery.RowsOffset, "RowsOffset should be -1");
			Assert.AreEqual((QuerySource)2, actualQuery.QuerySource, "QuerySource should be 2");
			Assert.AreEqual(2, actualQuery.Columns.Items.Count, "Should have 2 columns");

			// Verify Count aggregation structure
			var countColumn = actualQuery.Columns.Items.FirstOrDefault(c => c.Key == "ContactCount");
			Assert.IsNotNull(countColumn.Value, "ContactCount column should exist");
			Assert.AreEqual(EntitySchemaQueryExpressionType.Function, countColumn.Value.Expression.ExpressionType);
			Assert.AreEqual(FunctionType.Aggregation, countColumn.Value.Expression.FunctionType);
			Assert.AreEqual(AggregationType.Count, countColumn.Value.Expression.AggregationType);
			Assert.IsNotNull(countColumn.Value.Expression.FunctionArgument);
			Assert.AreEqual("Id", countColumn.Value.Expression.FunctionArgument.ColumnPath);

			// Verify Account grouping key structure
			var accountColumn = actualQuery.Columns.Items.FirstOrDefault(c => c.Key == "Account");
			Assert.IsNotNull(accountColumn.Value, "Account column should exist");
			Assert.AreEqual(EntitySchemaQueryExpressionType.SchemaColumn, accountColumn.Value.Expression.ExpressionType);
			Assert.AreEqual("Account", accountColumn.Value.Expression.ColumnPath);
		}
	}
}
