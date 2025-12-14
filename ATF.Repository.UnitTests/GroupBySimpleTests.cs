namespace ATF.Repository.UnitTests
{
	using System;
	using System.Linq;
	using ATF.Repository.Providers;
	using ATF.Repository.Replicas;
	using ATF.Repository.UnitTests.Models;
	using ATF.Repository.UnitTests.Utilities;
	using Newtonsoft.Json;
	using NSubstitute;
	using NUnit.Framework;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;
	using FunctionType = Terrasoft.Nui.ServiceModel.DataContract.FunctionType;
	using QuerySource = Terrasoft.Nui.ServiceModel.DataContract.QuerySource;

	[TestFixture]
	public class GroupBySimpleTests
	{
		private IDataProvider _dataProvider;
		private IAppDataContext _appDataContext;

		[SetUp]
		public void SetUp()
		{
			_dataProvider = Substitute.For<IDataProvider>();
			_appDataContext = AppDataContextFactory.GetAppDataContext(_dataProvider);
		}

		[Test]
		public void GroupBy_SingleKey_WithCountAggregation_GeneratesCorrectQuery()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - GroupBy 
			var result = _appDataContext.Models<TypedTestModel>()
				.GroupBy(x => new { x.LookupValue }, (groupBy, items) => new
				{
					groupBy.LookupValue,
					Count = items.Count()
				})
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);
			Assert.AreEqual("TypedTestModel", actualQuery.RootSchemaName);

			// Check GroupBy-specific parameters 
			Assert.AreEqual(false, actualQuery.IsPageable, "IsPageable should be false for GroupBy");
			Assert.AreEqual(-1, actualQuery.RowsOffset, "RowsOffset should be -1 for GroupBy");
			Assert.AreEqual((QuerySource)2, actualQuery.QuerySource, "QuerySource should be 2 for GroupBy");

			// Check columns count (2: aggregation + grouping key)
			Assert.AreEqual(2, actualQuery.Columns.Items.Count, "Should have exactly 2 columns: Count aggregation and LookupValue key");

			// Check Count aggregation column
			var countColumn = actualQuery.Columns.Items.FirstOrDefault(c => c.Key == "Count");
			Assert.IsNotNull(countColumn.Value, "Count column should exist");
			Assert.AreEqual(EntitySchemaQueryExpressionType.Function, countColumn.Value.Expression.ExpressionType, "Count should be Function type");
			Assert.AreEqual(FunctionType.Aggregation, countColumn.Value.Expression.FunctionType, "Count should be Aggregation function");
			Assert.AreEqual(AggregationType.Count, countColumn.Value.Expression.AggregationType, "Should be Count aggregation");
			Assert.IsNotNull(countColumn.Value.Expression.FunctionArgument, "FunctionArgument should exist");
			Assert.AreEqual(EntitySchemaQueryExpressionType.SchemaColumn, countColumn.Value.Expression.FunctionArgument.ExpressionType);
			Assert.AreEqual("Id", countColumn.Value.Expression.FunctionArgument.ColumnPath, "Count should aggregate Id column");

			// Check LookupValue grouping key column
			var lookupColumn = actualQuery.Columns.Items.FirstOrDefault(c => c.Key == "LookupValue");
			Assert.IsNotNull(lookupColumn.Value, "LookupValue column should exist");
			Assert.AreEqual(EntitySchemaQueryExpressionType.SchemaColumn, lookupColumn.Value.Expression.ExpressionType, "LookupValue should be SchemaColumn");
			Assert.AreEqual("LookupValue", lookupColumn.Value.Expression.ColumnPath, "Column path should be LookupValue");

			// Print actual JSON for debugging
			var json = JsonConvert.SerializeObject(actualQuery, Formatting.Indented);
			Console.WriteLine("Generated JSON:");
			Console.WriteLine(json);
		}

		[Test]
		public void GroupBy_MultipleKeys_WithMultipleAggregations_GeneratesCorrectQuery()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - GroupBy with multiple keys and aggregations
			var result = _appDataContext.Models<TypedTestModel>()
				.GroupBy(x => new { x.LookupValue, x.BooleanValue }, (groupBy, items) => new
				{
					groupBy.LookupValue,
					groupBy.BooleanValue,
					Count = items.Count(),
					TotalDecimal = items.Sum(y => y.DecimalValue)
				})
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);
			Assert.AreEqual("TypedTestModel", actualQuery.RootSchemaName);

			// Check GroupBy-specific parameters
			Assert.AreEqual(false, actualQuery.IsPageable);
			Assert.AreEqual(-1, actualQuery.RowsOffset);
			Assert.AreEqual((QuerySource)2, actualQuery.QuerySource);

			// Check columns count (4: 2 aggregations + 2 grouping keys)
			Assert.AreEqual(4, actualQuery.Columns.Items.Count, "Should have 4 columns: 2 aggregations + 2 keys");

			// Check Count aggregation
			var countColumn = actualQuery.Columns.Items.FirstOrDefault(c => c.Key == "Count");
			Assert.IsNotNull(countColumn.Value);
			Assert.AreEqual(AggregationType.Count, countColumn.Value.Expression.AggregationType);

			// Check Sum aggregation
			var sumColumn = actualQuery.Columns.Items.FirstOrDefault(c => c.Key == "TotalDecimal");
			Assert.IsNotNull(sumColumn.Value);
			Assert.AreEqual(EntitySchemaQueryExpressionType.Function, sumColumn.Value.Expression.ExpressionType);
			Assert.AreEqual(AggregationType.Sum, sumColumn.Value.Expression.AggregationType);
			Assert.AreEqual("DecimalValue", sumColumn.Value.Expression.FunctionArgument.ColumnPath);

			// Check grouping keys
			Assert.IsTrue(actualQuery.Columns.Items.ContainsKey("LookupValue"));
			Assert.IsTrue(actualQuery.Columns.Items.ContainsKey("BooleanValue"));

			// Print JSON for debugging
			Console.WriteLine("Generated JSON:");
			Console.WriteLine(JsonConvert.SerializeObject(actualQuery, Formatting.Indented));
		}

		[Test]
		public void GroupBy_AfterWhere_CombinesFiltersAndGroupBy()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - GroupBy after Where
			var result = _appDataContext.Models<TypedTestModel>()
				.Where(x => x.BooleanValue == true)
				.GroupBy(x => new { x.LookupValue }, (groupBy, items) => new
				{
					groupBy.LookupValue,
					Count = items.Count()
				})
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);

			// Check that Where filter exists
			Assert.IsNotNull(actualQuery.Filters);
			Assert.IsTrue(actualQuery.Filters.Items.Count > 0, "Where filter should be present");

			// Check GroupBy parameters
			Assert.AreEqual(false, actualQuery.IsPageable);
			Assert.AreEqual((QuerySource)2, actualQuery.QuerySource);

			// Check columns (only GroupBy columns, not all model columns)
			Assert.AreEqual(2, actualQuery.Columns.Items.Count, "Should have only GroupBy columns");
		}
	}
}
