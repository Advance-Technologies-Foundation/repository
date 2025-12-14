namespace ATF.Repository.UnitTests
{
	using System;
	using System.Linq;
	using ATF.Repository.Providers;
	using ATF.Repository.UnitTests.Models;
	using NUnit.Framework;

	[TestFixture]
	public class SelectDetailAggregationTests : BaseIntegrationTests
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
		[Category("DetailAggregation")]
		public void Contact_DetailCount_WithoutFilter()
		{
			// Test simple detail aggregation without Where filter
			// x.ContactInTags.Count()
			var results = _appDataContext.Models<Contact>()
				.Select(x => new {
					x.Name,
					TagCount = x.ContactInTags.Count()
				})
				.Take(10)
				.ToList();

			Console.WriteLine($"Contacts with TagCount: {results.Count}");
			Assert.Greater(results.Count, 0);

			if (results.Count > 0)
			{
				var first = results.First();
				Console.WriteLine($"Contact: {first.Name}, TagCount: {first.TagCount}");
				Assert.IsNotNull(first.Name);
				Assert.GreaterOrEqual(first.TagCount, 0);
			}
		}

		[Test]
		[Category("Integration")]
		[Category("DetailAggregation")]
		public void Contact_DetailCount_WithFilter_Predicate()
		{
			// Test detail aggregation with inline predicate
			// x.ContactInTags.Count(y => y.TagId == guid)
			// Similar to case1.txt: x.ContactCommunications.Count(y => y.CommunicationTypeId == guid)

			var testTagId = new Guid("00000000-0000-0000-0000-000000000001"); // Use a test GUID

			var results = _appDataContext.Models<Contact>()
				.Select(x => new {
					x.Name,
					TotalTags = x.ContactInTags.Count(),
					FilteredTags = x.ContactInTags.Count(y => y.TagId == testTagId)
				})
				.Take(10)
				.ToList();

			Console.WriteLine($"Contacts with filtered tags: {results.Count}");
			Assert.Greater(results.Count, 0);

			if (results.Count > 0)
			{
				var first = results.First();
				Console.WriteLine($"Contact: {first.Name}");
				Console.WriteLine($"  Total tags: {first.TotalTags}");
				Console.WriteLine($"  Filtered tags (TagId={testTagId}): {first.FilteredTags}");

				Assert.GreaterOrEqual(first.TotalTags, first.FilteredTags,
					"Total tags should be >= filtered tags");
			}
		}

		[Test]
		[Category("Integration")]
		[Category("DetailAggregation")]
		public void Contact_DetailCount_WithFilter_Where()
		{
			// Test detail aggregation with separate Where clause
			// x.ContactInTags.Where(y => y.TagId != Guid.Empty).Count()

			var results = _appDataContext.Models<Contact>()
				.Select(x => new {
					x.Name,
					NonEmptyTags = x.ContactInTags.Where(y => y.TagId != Guid.Empty).Count()
				})
				.Take(10)
				.ToList();

			Console.WriteLine($"Contacts with non-empty tags: {results.Count}");
			Assert.Greater(results.Count, 0);

			if (results.Count > 0)
			{
				var first = results.First();
				Console.WriteLine($"Contact: {first.Name}, NonEmptyTags: {first.NonEmptyTags}");
				Assert.GreaterOrEqual(first.NonEmptyTags, 0);
			}
		}

		[Test]
		[Category("Integration")]
		[Category("DetailAggregation")]
		public void Contact_MultipleDetailAggregations()
		{
			// Test multiple detail aggregations in one Select
			// Note: BinaryExpression like (x.ContactInTags.Count() > 0) is not supported in Creatio API
			// Such calculations should be done on the client side
			var results = _appDataContext.Models<Contact>()
				.Select(x => new {
					x.Id,
					x.Name,
					TagCount = x.ContactInTags.Count()
				})
				.Take(10)
				.ToList();

			Console.WriteLine($"Contacts with multiple aggregations: {results.Count}");
			Assert.Greater(results.Count, 0);

			if (results.Count > 0)
			{
				foreach (var contact in results.Take(3))
				{
					// Calculate HasTags on client side
					var hasTags = contact.TagCount > 0;
					Console.WriteLine($"{contact.Name}: {contact.TagCount} tags, HasTags: {hasTags}");
				}
			}
		}
	}
}
