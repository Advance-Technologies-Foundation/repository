namespace ATF.Repository.UnitTests
{
	using System;
	using System.Collections.Generic;
	using ATF.Repository.Providers;
	using ATF.Repository.UnitTests.Models;
	using NSubstitute;
	using NUnit.Framework;

	#region Class: AppDataContextSaveTests

	[TestFixture]
	public class AppDataContextSaveTests
	{
		#region Fields: Private

		private IDataProvider _dataProvider;
		private IAppDataContext _appDataContext;

		#endregion

		#region Methods: Private

		private IExecuteResponse CreateExecuteResponse(bool success, string errorMessage = null,
			List<IExecuteItemResponse> queryResults = null) {
			var response = Substitute.For<IExecuteResponse>();
			response.Success.Returns(success);
			response.ErrorMessage.Returns(errorMessage);
			response.QueryResults.Returns(queryResults ?? new List<IExecuteItemResponse>());
			return response;
		}

		private IExecuteItemResponse CreateExecuteItemResponse(bool success, string errorMessage = null) {
			var response = Substitute.For<IExecuteItemResponse>();
			response.Success.Returns(success);
			response.ErrorMessage.Returns(errorMessage);
			return response;
		}

		#endregion

		#region Methods: Public

		[SetUp]
		public void SetUp() {
			_dataProvider = Substitute.For<IDataProvider>();
			_appDataContext = AppDataContextFactory.GetAppDataContext(_dataProvider);
		}

		[Test]
		public void Save_WhenSuccessWithErrorMessage_ShouldReturnSuccessWithErrorMessage() {
			// Arrange
			_dataProvider.GetDefaultValues(Arg.Any<string>()).Returns(new DefaultValuesResponse() {
				Success = true,
				DefaultValues = new Dictionary<string, object>() {
					{ "Id", Guid.NewGuid() }
				}
			});

			var executeResponse = CreateExecuteResponse(true, "Success message");
			_dataProvider.BatchExecute(Arg.Any<List<IBaseQuery>>()).Returns(executeResponse);

			var model = _appDataContext.CreateModel<TypedTestModel>();
			model.StringValue = "Test";

			// Act
			var result = _appDataContext.Save();

			// Assert
			Assert.IsTrue(result.Success);
			Assert.AreEqual("Success message", result.ErrorMessage);
		}

		[Test]
		public void Save_WhenFailedWithErrorMessage_ShouldReturnErrorMessageFromResult() {
			// Arrange
			_dataProvider.GetDefaultValues(Arg.Any<string>()).Returns(new DefaultValuesResponse() {
				Success = true,
				DefaultValues = new Dictionary<string, object>() {
					{ "Id", Guid.NewGuid() }
				}
			});

			var executeResponse = CreateExecuteResponse(false, "Main error message");
			_dataProvider.BatchExecute(Arg.Any<List<IBaseQuery>>()).Returns(executeResponse);

			var model = _appDataContext.CreateModel<TypedTestModel>();
			model.StringValue = "Test";

			// Act
			var result = _appDataContext.Save();

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual("Main error message", result.ErrorMessage);
		}

		[Test]
		public void Save_WhenFailedWithEmptyErrorMessageAndQueryResults_ShouldReturnCombinedErrorMessages() {
			// Arrange
			_dataProvider.GetDefaultValues(Arg.Any<string>()).Returns(new DefaultValuesResponse() {
				Success = true,
				DefaultValues = new Dictionary<string, object>() {
					{ "Id", Guid.NewGuid() }
				}
			});

			var queryResults = new List<IExecuteItemResponse>() {
				CreateExecuteItemResponse(false, "Error in query 1"),
				CreateExecuteItemResponse(false, "Error in query 2")
			};
			var executeResponse = CreateExecuteResponse(false, "", queryResults);
			_dataProvider.BatchExecute(Arg.Any<List<IBaseQuery>>()).Returns(executeResponse);

			var model = _appDataContext.CreateModel<TypedTestModel>();
			model.StringValue = "Test";

			// Act
			var result = _appDataContext.Save();

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual("Error in query 1\nError in query 2", result.ErrorMessage);
		}

		[Test]
		public void Save_WhenFailedWithNullErrorMessageAndQueryResults_ShouldReturnCombinedErrorMessages() {
			// Arrange
			_dataProvider.GetDefaultValues(Arg.Any<string>()).Returns(new DefaultValuesResponse() {
				Success = true,
				DefaultValues = new Dictionary<string, object>() {
					{ "Id", Guid.NewGuid() }
				}
			});

			var queryResults = new List<IExecuteItemResponse>() {
				CreateExecuteItemResponse(false, "Error in query 1"),
				CreateExecuteItemResponse(false, "Error in query 2"),
				CreateExecuteItemResponse(false, "Error in query 3")
			};
			var executeResponse = CreateExecuteResponse(false, null, queryResults);
			_dataProvider.BatchExecute(Arg.Any<List<IBaseQuery>>()).Returns(executeResponse);

			var model = _appDataContext.CreateModel<TypedTestModel>();
			model.StringValue = "Test";

			// Act
			var result = _appDataContext.Save();

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual("Error in query 1\nError in query 2\nError in query 3", result.ErrorMessage);
		}

		[Test]
		public void Save_WhenFailedWithWhitespaceErrorMessageAndQueryResults_ShouldReturnCombinedErrorMessages() {
			// Arrange
			_dataProvider.GetDefaultValues(Arg.Any<string>()).Returns(new DefaultValuesResponse() {
				Success = true,
				DefaultValues = new Dictionary<string, object>() {
					{ "Id", Guid.NewGuid() }
				}
			});

			var queryResults = new List<IExecuteItemResponse>() {
				CreateExecuteItemResponse(false, "Query error 1"),
				CreateExecuteItemResponse(false, "Query error 2")
			};
			var executeResponse = CreateExecuteResponse(false, "   ", queryResults);
			_dataProvider.BatchExecute(Arg.Any<List<IBaseQuery>>()).Returns(executeResponse);

			var model = _appDataContext.CreateModel<TypedTestModel>();
			model.StringValue = "Test";

			// Act
			var result = _appDataContext.Save();

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual("Query error 1\nQuery error 2", result.ErrorMessage);
		}

		[Test]
		public void Save_WhenFailedWithEmptyErrorMessageAndEmptyQueryResults_ShouldReturnDefaultErrorMessage() {
			// Arrange
			_dataProvider.GetDefaultValues(Arg.Any<string>()).Returns(new DefaultValuesResponse() {
				Success = true,
				DefaultValues = new Dictionary<string, object>() {
					{ "Id", Guid.NewGuid() }
				}
			});

			var executeResponse = CreateExecuteResponse(false, "", new List<IExecuteItemResponse>());
			_dataProvider.BatchExecute(Arg.Any<List<IBaseQuery>>()).Returns(executeResponse);

			var model = _appDataContext.CreateModel<TypedTestModel>();
			model.StringValue = "Test";

			// Act
			var result = _appDataContext.Save();

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual("An unexpected error occurred during save operation", result.ErrorMessage);
		}

		[Test]
		public void Save_WhenNoChanges_ShouldReturnSuccessWithoutCallingBatchExecute() {
			// Arrange - читаємо існуючу модель але не змінюємо її
			var modelId = Guid.NewGuid();
			_dataProvider.GetItems(Arg.Any<ISelectQuery>()).Returns(new ItemsResponse() {
				Success = true,
				Items = new List<Dictionary<string, object>>() {
					new Dictionary<string, object>() {
						{ "Id", modelId },
						{ "StringValue", "Test" }
					}
				}
			});

			var model = _appDataContext.GetModel<TypedTestModel>(modelId);

			// Act
			var result = _appDataContext.Save();

			// Assert
			Assert.IsTrue(result.Success);
			_dataProvider.DidNotReceive().BatchExecute(Arg.Any<List<IBaseQuery>>());
		}

		[Test]
		public void Save_WhenFailedWithNullErrorMessageAndNullQueryResults_ShouldReturnDefaultErrorMessage() {
			// Arrange
			_dataProvider.GetDefaultValues(Arg.Any<string>()).Returns(new DefaultValuesResponse() {
				Success = true,
				DefaultValues = new Dictionary<string, object>() {
					{ "Id", Guid.NewGuid() }
				}
			});

			var executeResponse = CreateExecuteResponse(false, null, null);
			_dataProvider.BatchExecute(Arg.Any<List<IBaseQuery>>()).Returns(executeResponse);

			var model = _appDataContext.CreateModel<TypedTestModel>();
			model.StringValue = "Test";

			// Act
			var result = _appDataContext.Save();

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual("An unexpected error occurred during save operation", result.ErrorMessage);
		}

		#endregion
	}

	#endregion
}
