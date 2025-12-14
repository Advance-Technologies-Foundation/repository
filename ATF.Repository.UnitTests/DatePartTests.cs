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
	using DatePart = Terrasoft.Nui.ServiceModel.DataContract.DatePart;

	[TestFixture]
	public class DatePartTests
	{
		private IDataProvider _dataProvider;
		private IAppDataContext _appDataContext;

		[SetUp]
		public void SetUp()
		{
			_dataProvider = Substitute.For<IDataProvider>();
			_appDataContext = AppDataContextFactory.GetAppDataContext(_dataProvider);
		}

		#region Select with DatePart Tests

		[Test]
		public void Select_DateTimeField_Hour_GeneratesCorrectQuery()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - Select with DateTime.Hour
			var result = _appDataContext.Models<TypedTestModel>()
				.Select(x => new { Hour = x.DateTimeValue.Hour })
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);
			Assert.AreEqual("TypedTestModel", actualQuery.RootSchemaName);

			// Check columns count (1: Hour)
			Assert.AreEqual(1, actualQuery.Columns.Items.Count, "Should have exactly 1 column: Hour");

			// Check Hour column
			var hourColumn = actualQuery.Columns.Items.FirstOrDefault(c => c.Key == "Hour");
			Assert.IsNotNull(hourColumn.Value, "Hour column should exist");
			Assert.AreEqual(EntitySchemaQueryExpressionType.Function, hourColumn.Value.Expression.ExpressionType, "Hour should be Function type");
			Assert.AreEqual(FunctionType.DatePart, hourColumn.Value.Expression.FunctionType, "Should be DatePart function");
			Assert.AreEqual(DatePart.Hour, hourColumn.Value.Expression.DatePartType, "Should be Hour DatePart");

			// Check FunctionArgument
			Assert.IsNotNull(hourColumn.Value.Expression.FunctionArgument, "FunctionArgument should exist");
			Assert.AreEqual(EntitySchemaQueryExpressionType.SchemaColumn, hourColumn.Value.Expression.FunctionArgument.ExpressionType);
			Assert.AreEqual("DateTimeValue", hourColumn.Value.Expression.FunctionArgument.ColumnPath, "Should reference DateTimeValue column");

			// Print actual JSON for debugging
			var json = JsonConvert.SerializeObject(actualQuery, Formatting.Indented);
			Console.WriteLine("Generated JSON:");
			Console.WriteLine(json);
		}

		[Test]
		public void Select_DateTimeField_Year_GeneratesCorrectQuery()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - Select with DateTime.Year
			var result = _appDataContext.Models<TypedTestModel>()
				.Select(x => new { Year = x.DateTimeValue.Year })
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);

			// Check Year column
			var yearColumn = actualQuery.Columns.Items.FirstOrDefault(c => c.Key == "Year");
			Assert.IsNotNull(yearColumn.Value, "Year column should exist");
			Assert.AreEqual(EntitySchemaQueryExpressionType.Function, yearColumn.Value.Expression.ExpressionType);
			Assert.AreEqual(FunctionType.DatePart, yearColumn.Value.Expression.FunctionType);
			Assert.AreEqual(DatePart.Year, yearColumn.Value.Expression.DatePartType, "Should be Year DatePart");
			Assert.AreEqual("DateTimeValue", yearColumn.Value.Expression.FunctionArgument.ColumnPath);

			Console.WriteLine(JsonConvert.SerializeObject(actualQuery, Formatting.Indented));
		}

		[Test]
		public void Select_DateTimeField_Month_GeneratesCorrectQuery()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - Select with DateTime.Month
			var result = _appDataContext.Models<TypedTestModel>()
				.Select(x => new { Month = x.DateTimeValue.Month })
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);

			// Check Month column
			var monthColumn = actualQuery.Columns.Items.FirstOrDefault(c => c.Key == "Month");
			Assert.IsNotNull(monthColumn.Value, "Month column should exist");
			Assert.AreEqual(FunctionType.DatePart, monthColumn.Value.Expression.FunctionType);
			Assert.AreEqual(DatePart.Month, monthColumn.Value.Expression.DatePartType, "Should be Month DatePart");
		}

		[Test]
		public void Select_DateTimeField_Day_GeneratesCorrectQuery()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - Select with DateTime.Day
			var result = _appDataContext.Models<TypedTestModel>()
				.Select(x => new { Day = x.DateTimeValue.Day })
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);

			// Check Day column
			var dayColumn = actualQuery.Columns.Items.FirstOrDefault(c => c.Key == "Day");
			Assert.IsNotNull(dayColumn.Value, "Day column should exist");
			Assert.AreEqual(FunctionType.DatePart, dayColumn.Value.Expression.FunctionType);
			Assert.AreEqual(DatePart.Day, dayColumn.Value.Expression.DatePartType, "Should be Day DatePart");
		}

		[Test]
		public void Select_MultipleDateParts_GeneratesCorrectQuery()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - Select with multiple DateParts
			var result = _appDataContext.Models<TypedTestModel>()
				.Select(x => new
				{
					Year = x.DateTimeValue.Year,
					Month = x.DateTimeValue.Month,
					Day = x.DateTimeValue.Day,
					Hour = x.DateTimeValue.Hour
				})
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);
			Assert.AreEqual(4, actualQuery.Columns.Items.Count, "Should have 4 columns: Year, Month, Day, Hour");

			// Check each column
			Assert.IsTrue(actualQuery.Columns.Items.ContainsKey("Year"));
			Assert.IsTrue(actualQuery.Columns.Items.ContainsKey("Month"));
			Assert.IsTrue(actualQuery.Columns.Items.ContainsKey("Day"));
			Assert.IsTrue(actualQuery.Columns.Items.ContainsKey("Hour"));

			// Verify all are DatePart functions
			foreach (var column in actualQuery.Columns.Items.Values)
			{
				Assert.AreEqual(EntitySchemaQueryExpressionType.Function, column.Expression.ExpressionType);
				Assert.AreEqual(FunctionType.DatePart, column.Expression.FunctionType);
			}

			Console.WriteLine(JsonConvert.SerializeObject(actualQuery, Formatting.Indented));
		}

		[Test]
		public void Select_DatePartMixedWithRegularColumns_GeneratesCorrectQuery()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - Select with mix of DatePart and regular columns
			var result = _appDataContext.Models<TypedTestModel>()
				.Select(x => new
				{
					x.StringValue,
					Year = x.DateTimeValue.Year,
					x.IntValue
				})
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);
			Assert.AreEqual(3, actualQuery.Columns.Items.Count, "Should have 3 columns");

			// Check StringValue (regular column)
			var stringColumn = actualQuery.Columns.Items["StringValue"];
			Assert.AreEqual(EntitySchemaQueryExpressionType.SchemaColumn, stringColumn.Expression.ExpressionType);

			// Check Year (DatePart function)
			var yearColumn = actualQuery.Columns.Items["Year"];
			Assert.AreEqual(EntitySchemaQueryExpressionType.Function, yearColumn.Expression.ExpressionType);
			Assert.AreEqual(FunctionType.DatePart, yearColumn.Expression.FunctionType);

			// Check IntValue (regular column)
			var intColumn = actualQuery.Columns.Items["IntValue"];
			Assert.AreEqual(EntitySchemaQueryExpressionType.SchemaColumn, intColumn.Expression.ExpressionType);
		}

		[Test]
		public void Select_SingleDatePart_NotAnonymousType()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - Select single DatePart property (not anonymous object)
			var result = _appDataContext.Models<TypedTestModel>()
				.Select(x => x.DateTimeValue.Year)
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);
			Assert.AreEqual(1, actualQuery.Columns.Items.Count);

			var column = actualQuery.Columns.Items.Values.First();
			Assert.AreEqual(EntitySchemaQueryExpressionType.Function, column.Expression.ExpressionType);
			Assert.AreEqual(FunctionType.DatePart, column.Expression.FunctionType);
			Assert.AreEqual(DatePart.Year, column.Expression.DatePartType);
		}

		#endregion

		#region GroupBy with DatePart Tests

		[Test]
		public void GroupBy_DatePartKey_Hour_GeneratesCorrectQuery()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - GroupBy with DateTime.Hour as key
			var result = _appDataContext.Models<TypedTestModel>()
				.GroupBy(x => new { Hour = x.DateTimeValue.Hour }, (groupBy, items) => new
				{
					groupBy.Hour,
					Count = items.Count()
				})
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);
			Assert.AreEqual("TypedTestModel", actualQuery.RootSchemaName);

			// Check GroupBy-specific parameters
			Assert.AreEqual(false, actualQuery.IsPageable, "IsPageable should be false for GroupBy");
			Assert.AreEqual(-1, actualQuery.RowsOffset, "RowsOffset should be -1 for GroupBy");

			// Check columns count (2: Hour key + Count aggregation)
			Assert.AreEqual(2, actualQuery.Columns.Items.Count, "Should have 2 columns: Hour key and Count");

			// Check Hour column (grouping key with DatePart)
			var hourColumn = actualQuery.Columns.Items.FirstOrDefault(c => c.Key == "Hour");
			Assert.IsNotNull(hourColumn.Value, "Hour column should exist");
			Assert.AreEqual(EntitySchemaQueryExpressionType.Function, hourColumn.Value.Expression.ExpressionType, "Hour should be Function type");
			Assert.AreEqual(FunctionType.DatePart, hourColumn.Value.Expression.FunctionType, "Should be DatePart function");
			Assert.AreEqual(DatePart.Hour, hourColumn.Value.Expression.DatePartType, "Should be Hour DatePart");
			Assert.IsNotNull(hourColumn.Value.Expression.FunctionArgument);
			Assert.AreEqual("DateTimeValue", hourColumn.Value.Expression.FunctionArgument.ColumnPath);

			// Check Count aggregation
			var countColumn = actualQuery.Columns.Items.FirstOrDefault(c => c.Key == "Count");
			Assert.IsNotNull(countColumn.Value);
			Assert.AreEqual(EntitySchemaQueryExpressionType.Function, countColumn.Value.Expression.ExpressionType);
			Assert.AreEqual(FunctionType.Aggregation, countColumn.Value.Expression.FunctionType);

			Console.WriteLine(JsonConvert.SerializeObject(actualQuery, Formatting.Indented));
		}

		[Test]
		public void GroupBy_DatePartKey_Year_GeneratesCorrectQuery()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - GroupBy with DateTime.Year as key
			var result = _appDataContext.Models<TypedTestModel>()
				.GroupBy(x => new { Year = x.DateTimeValue.Year }, (groupBy, items) => new
				{
					groupBy.Year,
					Count = items.Count()
				})
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);

			var yearColumn = actualQuery.Columns.Items.FirstOrDefault(c => c.Key == "Year");
			Assert.IsNotNull(yearColumn.Value);
			Assert.AreEqual(FunctionType.DatePart, yearColumn.Value.Expression.FunctionType);
			Assert.AreEqual(DatePart.Year, yearColumn.Value.Expression.DatePartType);
		}

		[Test]
		public void GroupBy_MultipleDatePartKeys_GeneratesCorrectQuery()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - GroupBy with multiple DatePart keys
			var result = _appDataContext.Models<TypedTestModel>()
				.GroupBy(x => new
				{
					Year = x.DateTimeValue.Year,
					Month = x.DateTimeValue.Month
				}, (groupBy, items) => new
				{
					groupBy.Year,
					groupBy.Month,
					Count = items.Count()
				})
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);
			Assert.AreEqual(3, actualQuery.Columns.Items.Count, "Should have 3 columns: Year, Month, Count");

			// Check Year key
			var yearColumn = actualQuery.Columns.Items["Year"];
			Assert.AreEqual(FunctionType.DatePart, yearColumn.Expression.FunctionType);
			Assert.AreEqual(DatePart.Year, yearColumn.Expression.DatePartType);

			// Check Month key
			var monthColumn = actualQuery.Columns.Items["Month"];
			Assert.AreEqual(FunctionType.DatePart, monthColumn.Expression.FunctionType);
			Assert.AreEqual(DatePart.Month, monthColumn.Expression.DatePartType);

			Console.WriteLine(JsonConvert.SerializeObject(actualQuery, Formatting.Indented));
		}

		[Test]
		public void GroupBy_DatePartMixedWithRegularKey_GeneratesCorrectQuery()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - GroupBy with mix of DatePart and regular column keys
			var result = _appDataContext.Models<TypedTestModel>()
				.GroupBy(x => new
				{
					x.LookupValue,
					Year = x.DateTimeValue.Year
				}, (groupBy, items) => new
				{
					groupBy.LookupValue,
					groupBy.Year,
					Count = items.Count()
				})
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);
			Assert.AreEqual(3, actualQuery.Columns.Items.Count);

			// Check LookupValue (regular column)
			var lookupColumn = actualQuery.Columns.Items["LookupValue"];
			Assert.AreEqual(EntitySchemaQueryExpressionType.SchemaColumn, lookupColumn.Expression.ExpressionType);

			// Check Year (DatePart function)
			var yearColumn = actualQuery.Columns.Items["Year"];
			Assert.AreEqual(EntitySchemaQueryExpressionType.Function, yearColumn.Expression.ExpressionType);
			Assert.AreEqual(FunctionType.DatePart, yearColumn.Expression.FunctionType);
		}

		[Test]
		public void GroupBy_DatePartKey_WithMultipleAggregations()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - GroupBy DatePart with multiple aggregations
			var result = _appDataContext.Models<TypedTestModel>()
				.GroupBy(x => new { Year = x.DateTimeValue.Year }, (groupBy, items) => new
				{
					groupBy.Year,
					Count = items.Count(),
					TotalDecimal = items.Sum(y => y.DecimalValue),
					AvgInt = items.Average(y => y.IntValue)
				})
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);
			Assert.AreEqual(4, actualQuery.Columns.Items.Count, "Should have 4 columns: 1 key + 3 aggregations");

			// Verify Year is DatePart
			var yearColumn = actualQuery.Columns.Items["Year"];
			Assert.AreEqual(FunctionType.DatePart, yearColumn.Expression.FunctionType);

			// Verify aggregations exist
			Assert.IsTrue(actualQuery.Columns.Items.ContainsKey("Count"));
			Assert.IsTrue(actualQuery.Columns.Items.ContainsKey("TotalDecimal"));
			Assert.IsTrue(actualQuery.Columns.Items.ContainsKey("AvgInt"));
		}

		[Test]
		public void GroupBy_DatePartKey_AfterWhere()
		{
			// Arrange
			SelectQueryReplica actualQuery = null;
			_dataProvider.GetItems(Arg.Do<ISelectQuery>(q => actualQuery = q as SelectQueryReplica))
				.Returns(new ItemsResponse { Success = true, Items = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>() });

			// Act - GroupBy DatePart after Where
			var result = _appDataContext.Models<TypedTestModel>()
				.Where(x => x.BooleanValue == true)
				.GroupBy(x => new { Year = x.DateTimeValue.Year }, (groupBy, items) => new
				{
					groupBy.Year,
					Count = items.Count()
				})
				.ToList();

			// Assert
			Assert.IsNotNull(actualQuery);

			// Check that Where filter exists
			Assert.IsNotNull(actualQuery.Filters);
			Assert.IsTrue(actualQuery.Filters.Items.Count > 0, "Where filter should be present");

			// Check GroupBy columns
			Assert.AreEqual(2, actualQuery.Columns.Items.Count);

			// Verify Year is DatePart
			var yearColumn = actualQuery.Columns.Items["Year"];
			Assert.AreEqual(FunctionType.DatePart, yearColumn.Expression.FunctionType);
		}

		#endregion
	}
}
