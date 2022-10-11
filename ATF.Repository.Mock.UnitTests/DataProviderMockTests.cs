namespace ATF.Repository.Mock.UnitTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using ATF.Repository.Mock.UnitTests.Models;
	using ATF.Repository.Providers;
	using NUnit.Framework;

	public class DataProviderMockTests
	{
		private DataProviderMock _dataProviderMock;
		private IAppDataContext _appDataContext;
		private static string _sysSettingCode = "SysSettingCode";

		[SetUp]
		public void Setup() {
			_dataProviderMock = new DataProviderMock();
			_appDataContext = AppDataContextFactory.GetAppDataContext(_dataProviderMock);
		}

		[Test]
		public void MockDefaultValues_WhenSetAvailableStringDefaultValue_ShouldUseDefaultValuesForNewModel() {
			// Arrange
			var expectedValue = "ExpectedValue";
			_dataProviderMock.MockDefaultValues("TypedTestModel").Returns(new Dictionary<string, object>() {
				{"StringValue", expectedValue}
			});

			// Act
			var model = _appDataContext.CreateModel<TypedTestModel>();

			// Assert
			Assert.AreEqual(expectedValue, model.StringValue);

		}

		[Test]
		public void MockDefaultValues_WhenSetAvailableIntDefaultValue_ShouldUseDefaultValuesForNewModel() {
			// Arrange
			var expectedValue = 10;
			_dataProviderMock.MockDefaultValues("TypedTestModel").Returns(new Dictionary<string, object>() {
				{"IntValue", expectedValue}
			});

			// Act
			var model = _appDataContext.CreateModel<TypedTestModel>();

			// Assert
			Assert.AreEqual(expectedValue, model.IntValue);
		}

		[Test]
		public void MockDefaultValues_WhenSetAvailableDecimalDefaultValue_ShouldUseDefaultValuesForNewModel() {
			// Arrange
			var expectedValue = 10.11m;
			_dataProviderMock.MockDefaultValues("TypedTestModel").Returns(new Dictionary<string, object>() {
				{"DecimalValue", expectedValue}
			});

			// Act
			var model = _appDataContext.CreateModel<TypedTestModel>();

			// Assert
			Assert.AreEqual(expectedValue, model.DecimalValue);
		}

		[Test]
		public void MockDefaultValues_WhenSetAvailableDateTimeDefaultValue_ShouldUseDefaultValuesForNewModel() {
			// Arrange
			var expectedValue = DateTime.Now;
			_dataProviderMock.MockDefaultValues("TypedTestModel").Returns(new Dictionary<string, object>() {
				{"DateTimeValue", expectedValue}
			});

			// Act
			var model = _appDataContext.CreateModel<TypedTestModel>();

			// Assert
			Assert.AreEqual(expectedValue, model.DateTimeValue);
		}

		[Test]
		public void MockDefaultValues_WhenSetAvailableGuidDefaultValue_ShouldUseDefaultValuesForNewModel() {
			// Arrange
			var expectedValue = Guid.NewGuid();
			_dataProviderMock.MockDefaultValues("TypedTestModel").Returns(new Dictionary<string, object>() {
				{"GuidValue", expectedValue}
			});

			// Act
			var model = _appDataContext.CreateModel<TypedTestModel>();

			// Assert
			Assert.AreEqual(expectedValue, model.GuidValueId);
		}

		[Test]
		public void MockDefaultValues_WhenSetAvailableBooleanDefaultValue_ShouldUseDefaultValuesForNewModel() {
			// Arrange
			var expectedValue = true;
			_dataProviderMock.MockDefaultValues("TypedTestModel").Returns(new Dictionary<string, object>() {
				{"BooleanValue", expectedValue}
			});

			// Act
			var model = _appDataContext.CreateModel<TypedTestModel>();

			// Assert
			Assert.AreEqual(expectedValue, model.BooleanValue);
		}

		[Test]
		public void MockDefaultValues_WhenSetAvailableSimpleLookupDefaultValue_ShouldUseDefaultValuesForNewModel() {
			// Arrange
			var expectedValue = Guid.NewGuid();
			_dataProviderMock.MockDefaultValues("TypedTestModel").Returns(new Dictionary<string, object>() {
				{"LookupValue", expectedValue}
			});

			// Act
			var model = _appDataContext.CreateModel<TypedTestModel>();

			// Assert
			Assert.AreEqual(expectedValue, model.LookupValueId);
		}

		[Test]
		public void MockDefaultValues_WhenSetEmptyDefaultValue_ShouldUseDefaultValuesForNewModel() {
			// Arrange
			_dataProviderMock.MockDefaultValues("TypedTestModel").Returns(new Dictionary<string, object>() {
			});

			// Act
			var model = _appDataContext.CreateModel<TypedTestModel>();

			// Assert
			Assert.IsTrue(model.Id != Guid.Empty);
		}

		[Test]
		public void MockDefaultValues_WhenSetFailureResponse_ShouldUseEmptyDefaultValuesForNewModel() {
			// Arrange
			_dataProviderMock.MockDefaultValues("TypedTestModel").Returns(false, "ErrorMessage");

			// Act
			var model = _appDataContext.CreateModel<TypedTestModel>();

			// Assert
			Assert.IsTrue(model.Id != Guid.Empty);
		}

		[Test]
		public void MockDefaultValues_WhenSetAvailableDefaultValue_ShouldCallReceiveHandler() {
			// Arrange
			var expectedValue = Guid.NewGuid();
			var isMethodCalled = false;
			_dataProviderMock.MockDefaultValues("TypedTestModel").Returns(new Dictionary<string, object>() {
				{"LookupValue", expectedValue}
			}).ReceiveHandler(x => {
				isMethodCalled = true;
			});

			// Act
			var model = _appDataContext.CreateModel<TypedTestModel>();

			// Assert
			Assert.IsTrue(isMethodCalled);
		}

		[Test]
		public void MockDefaultValues_WhenSetAvailableDefaultValueAndNotCreateNewModel_ShouldCallReceiveHandler() {
			// Arrange
			var expectedValue = Guid.NewGuid();
			var isMethodCalled = false;
			_dataProviderMock.MockDefaultValues("TypedTestModel").Returns(new Dictionary<string, object>() {
				{"LookupValue", expectedValue}
			}).ReceiveHandler(x => {
				isMethodCalled = true;
			});

			// Assert
			Assert.IsFalse(isMethodCalled);
		}

		[Test]
		public void MockGetItems_WhenUseSimpleStringQueryFilter_ShouldReturnExpectedValues() {
			// Arrange
			var expectedId = Guid.NewGuid();
			var expectedString = "expectedString";
			var filterValue = "expectedString";
			var mock =_dataProviderMock.MockItems("TypedTestModel").FilterHas(filterValue).Returns(new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{"Id", expectedId},
					{"StringValue", expectedString}
				}
			});

			// Act
			var models = _appDataContext.Models<TypedTestModel>().Where(x => x.StringValue == filterValue).ToList();

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(1, models.Count());
			Assert.AreEqual(expectedId, models.First().Id);
			Assert.AreEqual(expectedString, models.First().StringValue);
		}

		[Test]
		public void MockGetItems_WhenUseSimpleIntQueryFilter_ShouldReturnExpectedValues() {
			// Arrange
			var expectedId = Guid.NewGuid();
			var expectedString = "expectedString";
			var filterValue = 100;
			var mock =_dataProviderMock.MockItems("TypedTestModel").FilterHas(filterValue).Returns(new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{"Id", expectedId},
					{"StringValue", expectedString}
				}
			});

			// Act
			var models = _appDataContext.Models<TypedTestModel>().Where(x => x.IntValue == filterValue).ToList();

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(1, models.Count());
			Assert.AreEqual(expectedId, models.First().Id);
			Assert.AreEqual(expectedString, models.First().StringValue);
		}

		[Test]
		public void MockGetItems_WhenUseSimpleDecimalQueryFilter_ShouldReturnExpectedValues() {
			// Arrange
			var expectedId = Guid.NewGuid();
			var expectedString = "expectedString";
			var filterValue = 100m;
			var mock =_dataProviderMock.MockItems("TypedTestModel").FilterHas(filterValue).Returns(new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{"Id", expectedId},
					{"StringValue", expectedString}
				}
			});

			// Act
			var models = _appDataContext.Models<TypedTestModel>().Where(x => x.DecimalValue == filterValue).ToList();

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(1, models.Count());
			Assert.AreEqual(expectedId, models.First().Id);
			Assert.AreEqual(expectedString, models.First().StringValue);
		}

		[Test]
		public void MockGetItems_WhenUseSimpleDateTimeQueryFilter_ShouldReturnExpectedValues() {
			// Arrange
			var expectedId = Guid.NewGuid();
			var expectedString = "expectedString";
			var filterValue = new DateTime(2021, 5, 3, 10, 11, 27);
			var hasFilterValue = new DateTime(2021, 5, 3, 10, 11, 27);
			var mock =_dataProviderMock.MockItems("TypedTestModel").FilterHas(hasFilterValue).Returns(new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{"Id", expectedId},
					{"StringValue", expectedString}
				}
			});

			// Act
			var models = _appDataContext.Models<TypedTestModel>().Where(x => x.DateTimeValue == filterValue).ToList();

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(1, models.Count());
			Assert.AreEqual(expectedId, models.First().Id);
			Assert.AreEqual(expectedString, models.First().StringValue);
		}

		[Test]
		public void MockGetItems_WhenUseSimpleGuidQueryFilter_ShouldReturnExpectedValues() {
			// Arrange
			var expectedId = Guid.NewGuid();
			var expectedString = "expectedString";
			var filterValue = Guid.NewGuid();
			var hasFilterValue = new Guid(filterValue.ToString());
			var mock =_dataProviderMock.MockItems("TypedTestModel").FilterHas(hasFilterValue).Returns(new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{"Id", expectedId},
					{"StringValue", expectedString}
				}
			});

			// Act
			var models = _appDataContext.Models<TypedTestModel>().Where(x => x.GuidValueId == filterValue).ToList();

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(1, models.Count());
			Assert.AreEqual(expectedId, models.First().Id);
			Assert.AreEqual(expectedString, models.First().StringValue);
		}

		[Test]
		public void MockGetItems_WhenUseSimpleNullableDateTimeQueryFilter_ShouldReturnExpectedValues() {
			// Arrange
			var expectedId = Guid.NewGuid();
			var expectedString = "expectedString";
			var mock =_dataProviderMock.MockItems("TypedTestModel").FilterHas(null).Returns(new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{"Id", expectedId},
					{"StringValue", expectedString}
				}
			});

			// Act
			var models = _appDataContext.Models<TypedTestModel>().Where(x => x.DateTimeValue == null).ToList();

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(1, models.Count());
			Assert.AreEqual(expectedId, models.First().Id);
			Assert.AreEqual(expectedString, models.First().StringValue);
		}

		[Test]
		public void MockGetItems_WhenUseSimpleShortBooleanTrueQueryFilter_ShouldReturnExpectedValues() {
			// Arrange
			var expectedId = Guid.NewGuid();
			var expectedString = "expectedString";
			var mock =_dataProviderMock.MockItems("TypedTestModel").FilterHas(true).Returns(new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{"Id", expectedId},
					{"StringValue", expectedString}
				}
			});

			// Act
			var models = _appDataContext.Models<TypedTestModel>().Where(x => x.BooleanValue).ToList();

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(1, models.Count());
			Assert.AreEqual(expectedId, models.First().Id);
			Assert.AreEqual(expectedString, models.First().StringValue);
		}

		[Test]
		public void MockGetItems_WhenUseSimpleShortBooleanFalseQueryFilter_ShouldReturnExpectedValues() {
			// Arrange
			var expectedId = Guid.NewGuid();
			var expectedString = "expectedString";
			var mock =_dataProviderMock.MockItems("TypedTestModel").FilterHas(true).Returns(new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{"Id", expectedId},
					{"StringValue", expectedString}
				}
			});

			// Act
			var models = _appDataContext.Models<TypedTestModel>().Where(x => !x.BooleanValue).ToList();

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(1, models.Count());
			Assert.AreEqual(expectedId, models.First().Id);
			Assert.AreEqual(expectedString, models.First().StringValue);
		}

		[Test]
		public void MockGetItems_WhenUseSimpleLookupStringQueryFilter_ShouldReturnExpectedValues() {
			// Arrange
			var expectedId = Guid.NewGuid();
			var expectedString = "expectedString";
			var filterValue = "expectedString";
			var mock =_dataProviderMock.MockItems("TypedTestModel").FilterHas(filterValue).Returns(new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{"Id", expectedId},
					{"StringValue", expectedString}
				}
			});

			// Act
			var models = _appDataContext.Models<TypedTestModel>().Where(x => x.Parent.StringValue != filterValue).ToList();

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(1, models.Count());
			Assert.AreEqual(expectedId, models.First().Id);
			Assert.AreEqual(expectedString, models.First().StringValue);
		}

		[Test]
		public void MockGetItems_WhenUseContainsStringQueryFilter_ShouldReturnExpectedValues() {
			// Arrange
			var expectedId = Guid.NewGuid();
			var expectedString = "expectedString";
			var filterValue = "expectedString";
			var mock =_dataProviderMock.MockItems("TypedTestModel").FilterHas(filterValue).Returns(new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{"Id", expectedId},
					{"StringValue", expectedString}
				}
			});

			// Act
			var models = _appDataContext.Models<TypedTestModel>().Where(x => x.Parent.StringValue.Contains(filterValue)).ToList();

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(1, models.Count());
			Assert.AreEqual(expectedId, models.First().Id);
			Assert.AreEqual(expectedString, models.First().StringValue);
		}

		[Test]
		public void MockGetItems_WhenUseStartWithStringQueryFilter_ShouldReturnExpectedValues() {
			// Arrange
			var expectedId = Guid.NewGuid();
			var expectedString = "expectedString";
			var filterValue = "expectedString";
			var mock =_dataProviderMock.MockItems("TypedTestModel").FilterHas(filterValue).Returns(new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{"Id", expectedId},
					{"StringValue", expectedString}
				}
			});

			// Act
			var models = _appDataContext.Models<TypedTestModel>().Where(x => x.Parent.StringValue.StartsWith(filterValue)).ToList();

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(1, models.Count());
			Assert.AreEqual(expectedId, models.First().Id);
			Assert.AreEqual(expectedString, models.First().StringValue);
		}

		[Test]
		public void MockGetItems_WhenUseEndsWithStringQueryFilter_ShouldReturnExpectedValues() {
			// Arrange
			var expectedId = Guid.NewGuid();
			var expectedString = "expectedString";
			var filterValue = "expectedString";
			var mock =_dataProviderMock.MockItems("TypedTestModel").FilterHas(filterValue).Returns(new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{"Id", expectedId},
					{"StringValue", expectedString}
				}
			});

			// Act
			var models = _appDataContext.Models<TypedTestModel>().Where(x => x.Parent.StringValue.EndsWith(filterValue)).ToList();

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(1, models.Count());
			Assert.AreEqual(expectedId, models.First().Id);
			Assert.AreEqual(expectedString, models.First().StringValue);
		}

		[Test]
		public void MockGetItems_WhenUseInStringQueryFilter_ShouldReturnExpectedValues() {
			// Arrange
			var expectedId = Guid.NewGuid();
			var expectedString = "expectedString";
			var filterValues = new List<string>() { "expectedString1", "expectedString2", "expectedString3" };
			var mock = _dataProviderMock.MockItems("TypedTestModel").FilterHas(filterValues[0]).FilterHas(filterValues[1])
				.FilterHas(filterValues[2]).Returns(new List<Dictionary<string, object>>() {
					new Dictionary<string, object>() {
						{ "Id", expectedId },
						{ "StringValue", expectedString }
					}
				});

			// Act
			var models = _appDataContext.Models<TypedTestModel>().Where(x => filterValues.Contains(x.Parent.StringValue)).ToList();

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(1, models.Count());
			Assert.AreEqual(expectedId, models.First().Id);
			Assert.AreEqual(expectedString, models.First().StringValue);
		}

		[Test]
		public void MockGetItems_WhenUseSubQueryWithInStringQueryFilter_ShouldReturnExpectedValues() {
			// Arrange
			var expectedId = Guid.NewGuid();
			var expectedString = "expectedString";
			var filterValues = new List<string>() { "expectedString1", "expectedString2", "expectedString3" };
			var mock = _dataProviderMock.MockItems("TypedTestModel").FilterHas(filterValues[0]).FilterHas(filterValues[1])
				.FilterHas(filterValues[2]).Returns(new List<Dictionary<string, object>>() {
					new Dictionary<string, object>() {
						{ "Id", expectedId },
						{ "StringValue", expectedString }
					}
				});

			// Act
			var models = _appDataContext.Models<TypedTestModel>()
				.Where(x => x.DetailModels.Any(y => filterValues.Contains(y.Parent.StringValue))).ToList();

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(1, models.Count());
			Assert.AreEqual(expectedId, models.First().Id);
			Assert.AreEqual(expectedString, models.First().StringValue);
		}

		[Test]
		public void MockGetItems_WhenUseSimpleAggregationAnyColumn_ShouldReturnExpectedValues() {
			// Arrange
			var filterValues = new List<string>() { "expectedString1", "expectedString2", "expectedString3" };
			var mock = _dataProviderMock.MockScalar("TypedTestModel", AggregationScalarType.Any).FilterHas(filterValues[0]).FilterHas(filterValues[1])
				.FilterHas(filterValues[2]).Returns(1);

			// Act
			var actualValue = _appDataContext
				.Models<TypedTestModel>().Any(x => x.DetailModels.Any(y => filterValues.Contains(y.Parent.StringValue)));

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.IsTrue(actualValue);
		}

		[Test]
		public void MockGetItems_WhenUseSimpleAggregationSumColumn_ShouldReturnExpectedValues() {
			// Arrange
			var expectedValue = 10m;
			var filterValues = new List<string>() { "expectedString1", "expectedString2", "expectedString3" };
			var mock = _dataProviderMock.MockScalar("TypedTestModel", AggregationScalarType.Sum).FilterHas(filterValues[0]).FilterHas(filterValues[1])
				.FilterHas(filterValues[2]).Returns(expectedValue);

			// Act
			var actualValue = _appDataContext
				.Models<TypedTestModel>().Where(x => x.DetailModels.Any(y => filterValues.Contains(y.Parent.StringValue))).Sum(x=>x.DecimalValue);

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(expectedValue, actualValue);
		}

		[Test]
		[TestCase(true, 1, 15.5)]
		[TestCase(false, 0, 0)]
		public void MockGetItems_WhenUseSimpleAggregationAvgColumn_ShouldReturnExpectedValues(bool enabledMock, int receivedCount, decimal expectedValue) {
			// Arrange
			var filterValues = new List<string>() { "expectedString1", "expectedString2", "expectedString3" };
			var mock = _dataProviderMock.MockScalar("TypedTestModel", AggregationScalarType.Avg).FilterHas(filterValues[0]).FilterHas(filterValues[1])
				.FilterHas(filterValues[2]).Returns(expectedValue);
			mock.Enabled = enabledMock;
			// Act
			var actualValue = _appDataContext
				.Models<TypedTestModel>().Where(x => x.DetailModels.Any(y => filterValues.Contains(y.Parent.StringValue))).Average(x=>x.DecimalValue);

			// Assert
			Assert.AreEqual(receivedCount, mock.ReceivedCount);
			Assert.AreEqual(expectedValue, actualValue);
		}

		[Test]
		public void MockGetItems_WhenUseSimpleAggregationCountColumn_ShouldReturnExpectedValues() {
			// Arrange
			var expectedValue = 16;
			var filterValues = new List<string>() { "expectedString1", "expectedString2", "expectedString3" };
			var mock = _dataProviderMock.MockScalar("TypedTestModel", AggregationScalarType.Count).FilterHas(filterValues[0]).FilterHas(filterValues[1])
				.FilterHas(filterValues[2]).Returns(expectedValue);

			// Act
			var actualValue = _appDataContext
				.Models<TypedTestModel>().Count(x => x.DetailModels.Any(y => filterValues.Contains(y.Parent.StringValue)));

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(expectedValue, actualValue);
		}

		[Test]
		public void MockGetItems_WhenUseSimpleAggregationMaxColumn_ShouldReturnExpectedValues() {
			// Arrange
			var expectedValue = 16.3m;
			var filterValues = new List<string>() { "expectedString1", "expectedString2", "expectedString3" };
			var mock = _dataProviderMock.MockScalar("TypedTestModel", AggregationScalarType.Max).FilterHas(filterValues[0]).FilterHas(filterValues[1])
				.FilterHas(filterValues[2]).Returns(expectedValue);

			// Act
			var actualValue = _appDataContext
				.Models<TypedTestModel>().Where(x => x.DetailModels.Any(y => filterValues.Contains(y.Parent.StringValue))).Max(x=>x.DecimalValue);

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(expectedValue, actualValue);
		}

		[Test]
		public void MockGetItems_WhenUseSimpleAggregationMinColumn_ShouldReturnExpectedValues() {
			// Arrange
			var expectedValue = 16.3m;
			var filterValues = new List<string>() { "expectedString1", "expectedString2", "expectedString3" };
			var mock = _dataProviderMock.MockScalar("TypedTestModel", AggregationScalarType.Min).FilterHas(filterValues[0]).FilterHas(filterValues[1])
				.FilterHas(filterValues[2]).Returns(expectedValue);

			// Act
			var actualValue = _appDataContext
				.Models<TypedTestModel>().Where(x => x.DetailModels.Any(y => filterValues.Contains(y.Parent.StringValue))).Min(x=>x.DecimalValue);

			// Assert
			Assert.AreEqual(1, mock.ReceivedCount);
			Assert.AreEqual(expectedValue, actualValue);
		}

		[Test]
		[TestCase(true, 1)]
		[TestCase(false, 0)]
		public void MockInsert_WhenUseChangedValueHasWithoutParameterName_ShouldReceiveHandler(bool needCallSave, int receivedCount) {
			// Arrange
			var expectedStringValue = "StringValue";
			var expectedIntValue = 10;
			var expectedDecimalValue = 10.11m;
			var expectedDateTimeValue = new DateTime(2021, 12, 23, 12, 15, 10);
			var expectedBooleanValue = true;
			var expectedGuidValue = Guid.NewGuid();
			var expectedLookupValue = Guid.NewGuid();

			var mock = _dataProviderMock.MockSavingItem("TypedTestModel", SavingOperation.Insert)
				.ChangedValueHas(expectedBooleanValue).ChangedValueHas(expectedDecimalValue)
				.ChangedValueHas(expectedGuidValue).ChangedValueHas(expectedIntValue)
				.ChangedValueHas(expectedLookupValue).ChangedValueHas(expectedStringValue)
				.ChangedValueHas(expectedDateTimeValue);

			// Act
			var model = _appDataContext.CreateModel<TypedTestModel>();
			model.StringValue = expectedStringValue;
			model.IntValue = expectedIntValue;
			model.DecimalValue = expectedDecimalValue;
			model.DateTimeValue = expectedDateTimeValue;
			model.BooleanValue = expectedBooleanValue;
			model.LookupValueId = expectedLookupValue;
			model.GuidValueId = expectedGuidValue;
			if (needCallSave) {
				_appDataContext.Save();
			}

			// Assert
			Assert.AreEqual(receivedCount, mock.ReceivedCount);
		}

		[Test]
		[TestCase(true, 1)]
		[TestCase(false, 0)]
		public void MockInsert_WhenUseChangedValueHasWithParameterName_ShouldReceiveHandler(bool needCallSave, int receivedCount) {
			// Arrange
			var expectedStringValue = "StringValue";
			var expectedIntValue = 10;
			var expectedDecimalValue = 10.11m;
			var expectedDateTimeValue = new DateTime(2021, 12, 23, 12, 15, 10);
			var expectedBooleanValue = true;
			var expectedGuidValue = Guid.NewGuid();
			var expectedLookupValue = Guid.NewGuid();

			var mock = _dataProviderMock.MockSavingItem("TypedTestModel", SavingOperation.Insert)
				.ChangedValueHas("BooleanValue", expectedBooleanValue)
				.ChangedValueHas("DecimalValue", expectedDecimalValue)
				.ChangedValueHas("GuidValue", expectedGuidValue)
				.ChangedValueHas("IntValue", expectedIntValue)
				.ChangedValueHas("LookupValue", expectedLookupValue)
				.ChangedValueHas("StringValue", expectedStringValue)
				.ChangedValueHas("DateTimeValue", expectedDateTimeValue);

			// Act
			var model = _appDataContext.CreateModel<TypedTestModel>();
			model.StringValue = expectedStringValue;
			model.IntValue = expectedIntValue;
			model.DecimalValue = expectedDecimalValue;
			model.DateTimeValue = expectedDateTimeValue;
			model.BooleanValue = expectedBooleanValue;
			model.LookupValueId = expectedLookupValue;
			model.GuidValueId = expectedGuidValue;
			if (needCallSave) {
				_appDataContext.Save();
			}

			// Assert
			Assert.AreEqual(receivedCount, mock.ReceivedCount);
		}

		[Test]
		[TestCase(true, 1)]
		[TestCase(false, 0)]
		public void MockUpdate_WithFilters_ShouldReceiveHandler(bool needCallSave, int receivedCount) {
			// Arrange

			var expectedId = Guid.NewGuid();
			var filterValue = "expectedString";
			var itemsMock =_dataProviderMock.MockItems("TypedTestModel").FilterHas(filterValue).Returns(new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{"Id", expectedId},
					{"StringValue", string.Empty}
				}
			});
			var expectedStringValue = "StringValue";
			var expectedIntValue = 10;
			var expectedDecimalValue = 10.11m;
			var expectedDateTimeValue = new DateTime(2021, 12, 23, 12, 15, 10);
			var expectedBooleanValue = true;
			var expectedGuidValue = Guid.NewGuid();
			var expectedLookupValue = Guid.NewGuid();
			var updatingMock = _dataProviderMock.MockSavingItem("TypedTestModel", SavingOperation.Update)
				.ChangedValueHas(expectedBooleanValue).ChangedValueHas(expectedDecimalValue)
				.ChangedValueHas(expectedGuidValue).ChangedValueHas(expectedIntValue)
				.ChangedValueHas(expectedLookupValue).ChangedValueHas(expectedStringValue)
				.ChangedValueHas(expectedDateTimeValue);

			// Act
			var models = _appDataContext.Models<TypedTestModel>().Where(x => x.StringValue == filterValue).ToList();
			var model = models.First();
			model.StringValue = expectedStringValue;
			model.IntValue = expectedIntValue;
			model.DecimalValue = expectedDecimalValue;
			model.DateTimeValue = expectedDateTimeValue;
			model.BooleanValue = expectedBooleanValue;
			model.LookupValueId = expectedLookupValue;
			model.GuidValueId = expectedGuidValue;
			if (needCallSave) {
				_appDataContext.Save();
			}

			// Assert
			Assert.AreEqual(1, itemsMock.ReceivedCount);
			Assert.AreEqual(receivedCount, updatingMock.ReceivedCount);
		}

		[Test]
		[TestCase(true, 1)]
		[TestCase(false, 0)]
		public void MockDelete_WithFilters_ShouldReceiveHandler(bool needCallSave, int receivedCount) {
			// Arrange

			var expectedId = Guid.NewGuid();
			const string filterValue = "expectedString";
			var itemsMock =_dataProviderMock.MockItems("TypedTestModel").FilterHas(filterValue).Returns(new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{"Id", expectedId},
					{"StringValue", string.Empty}
				}
			});

			var deletingMock = _dataProviderMock.MockSavingItem("TypedTestModel", SavingOperation.Delete)
				.ChangedValueHas(expectedId);

			// Act
			var models = _appDataContext.Models<TypedTestModel>().Where(x => x.StringValue == filterValue).ToList();
			var model = models.First();
			_appDataContext.DeleteModel(model);
			if (needCallSave) {
				_appDataContext.Save();
			}

			// Assert
			Assert.AreEqual(1, itemsMock.ReceivedCount);
			Assert.AreEqual(receivedCount, deletingMock.ReceivedCount);
		}

		public class SysSettingTestCase
		{
			public Type Type { get; set; }
			public object ExpectedValue { get; set; }
			public bool NeedMockRequest { get; set; }
		}


		protected static IEnumerable<object[]> GetSysSettingTestCases() {
			yield return new object[] {
				typeof(bool),
				true,
				true
			};
			yield return new object[] {
				typeof(bool),
				false,
				true
			};
			yield return new object[] {
				typeof(bool),
				false,
				false
			};
			yield return new object[] {
				typeof(int),
				-10,
				true
			};
			yield return new object[] {
				typeof(int),
				0,
				true
			};
			yield return new object[] {
				typeof(int),
				10,
				true
			};
			yield return new object[] {
				typeof(int),
				0,
				false
			};
			yield return new object[] {
				typeof(decimal),
				-10.11m,
				true
			};
			yield return new object[] {
				typeof(decimal),
				0m,
				true
			};
			yield return new object[] {
				typeof(decimal),
				10.11m,
				true
			};
			yield return new object[] {
				typeof(decimal),
				0m,
				false
			};
			yield return new object[] {
				typeof(string),
				"",
				true
			};
			yield return new object[] {
				typeof(string),
				"Expected value",
				true
			};
			yield return new object[] {
				typeof(string),
				"",
				false
			};
			yield return new object[] {
				typeof(DateTime),
				new DateTime(2021, 12, 28, 2, 40 , 0),
				true
			};
			yield return new object[] {
				typeof(DateTime),
				DateTime.MinValue,
				false
			};

			yield return new object[] {
				typeof(Guid),
				Guid.NewGuid(),
				true
			};
			yield return new object[] {
				typeof(Guid),
				Guid.Empty,
				true
			};
			yield return new object[] {
				typeof(Guid),
				Guid.Empty,
				false
			};
		}


		[Test, TestCaseSource("GetSysSettingTestCases")]
		public void MockGetSysSettingValue_ShouldReturnExpectedValue(Type valueType, object expectedValue, bool needMock) {
			// Arrange
			if (needMock) {
				_dataProviderMock.MockSysSettingValue(_sysSettingCode, expectedValue);
			}

			// Act
			var methodInfo = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.FirstOrDefault(instanceMethod => instanceMethod.Name == "TestTypedGetSysSettingValueMethod" && instanceMethod.ContainsGenericParameters);
			var method = methodInfo?.MakeGenericMethod(valueType);
			method?.Invoke(this, new object[] {_sysSettingCode, expectedValue});

		}

		public void TestTypedGetSysSettingValueMethod<T>(string code, object expectedValue) {
			var response = _appDataContext.GetSysSettingValue<T>(code);
			Assert.IsNotNull(response);
			Assert.AreEqual(expectedValue, response.Value);
			Assert.IsNull(response.ErrorMessage);
		}


		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void MockGetFeatureEnable_ShouldReturnExpectedValue(bool expectedValue, bool needMock) {
			// Arrange
			var featureCode = "FeatureCode";
			if (needMock) {
				_dataProviderMock.MockFeatureEnable(featureCode, expectedValue);
			}

			// Act
			var response = _appDataContext.GetFeatureEnabled(featureCode);

			// Assert
			Assert.IsNotNull(response);
			Assert.AreEqual(expectedValue, response.Enabled);
			Assert.IsNull(response.ErrorMessage);
		}
	}
}
