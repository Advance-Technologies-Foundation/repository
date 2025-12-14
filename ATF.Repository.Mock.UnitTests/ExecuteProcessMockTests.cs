namespace ATF.Repository.Mock.UnitTests
{
	using System;
	using System.Collections.Generic;
	using ATF.Repository.Attributes;
	using ATF.Repository.Providers;
	using NUnit.Framework;

	[BusinessProcess("TestSimpleParametersProcess")]
	public class TestSimpleParametersProcessModel : IBusinessProcess
	{
		[BusinessProcessParameter("StringParam", BusinessProcessParameterDirection.Input)]
		public string StringParam { get; set; }

		[BusinessProcessParameter("IntParam", BusinessProcessParameterDirection.Input)]
		public int IntParam { get; set; }

		[BusinessProcessParameter("DecimalParam", BusinessProcessParameterDirection.Input)]
		public decimal DecimalParam { get; set; }

		[BusinessProcessParameter("GuidParam", BusinessProcessParameterDirection.Input)]
		public Guid GuidParam { get; set; }

		[BusinessProcessParameter("BoolParam", BusinessProcessParameterDirection.Input)]
		public bool BoolParam { get; set; }

		[BusinessProcessParameter("DateTimeParam", BusinessProcessParameterDirection.Input)]
		public DateTime DateTimeParam { get; set; }

		[BusinessProcessParameter("OutputStringParam", BusinessProcessParameterDirection.Output)]
		public string OutputStringParam { get; set; }

		[BusinessProcessParameter("OutputIntParam", BusinessProcessParameterDirection.Output)]
		public int OutputIntParam { get; set; }
	}

	public class CustomParameter
	{
		[BusinessProcessParameter("Key", BusinessProcessParameterDirection.Bidirectional)]
		public string Key { get; set; }

		[BusinessProcessParameter("Value", BusinessProcessParameterDirection.Bidirectional)]
		public decimal Value { get; set; }

		[BusinessProcessParameter("Position", BusinessProcessParameterDirection.Bidirectional)]
		public int Position { get; set; }
	}

	[BusinessProcess("TestComplexParametersProcess")]
	public class TestComplexParametersProcessModel : IBusinessProcess
	{
		[BusinessProcessParameter("InputParams", BusinessProcessParameterDirection.Input)]
		public List<CustomParameter> InputParams { get; set; }

		[BusinessProcessParameter("OutputParams", BusinessProcessParameterDirection.Output)]
		public List<CustomParameter> OutputParams { get; set; }
	}

	[BusinessProcess("TestBidirectionalProcess")]
	public class TestBidirectionalProcessModel : IBusinessProcess
	{
		[BusinessProcessParameter("BidirectionalString", BusinessProcessParameterDirection.Bidirectional)]
		public string BidirectionalString { get; set; }

		[BusinessProcessParameter("BidirectionalInt", BusinessProcessParameterDirection.Bidirectional)]
		public int BidirectionalInt { get; set; }

		[BusinessProcessParameter("BidirectionalList", BusinessProcessParameterDirection.Bidirectional)]
		public List<CustomParameter> BidirectionalList { get; set; }
	}

	[BusinessProcess("TestErrorProcess")]
	public class TestErrorProcessModel : IBusinessProcess
	{
		[BusinessProcessParameter("InputParam", BusinessProcessParameterDirection.Input)]
		public string InputParam { get; set; }
	}

	public class ExecuteProcessMockTests
	{
		private DataProviderMock _dataProviderMock;
		private IAppProcessContext _appProcessContext;

		[SetUp]
		public void Setup()
		{
			_dataProviderMock = new DataProviderMock();
			_appProcessContext = AppProcessContextFactory.GetAppProcessContext(_dataProviderMock);
		}

		[Test]
		public void MockExecuteProcess_WithSimpleParameters_ShouldReturnExpectedValues()
		{
			// Arrange
			var processSchemaName = "TestSimpleParametersProcess";
			var inputString = "TestValue";
			var inputInt = 42;
			var inputDecimal = 10.5m;
			var inputGuid = Guid.NewGuid();
			var inputBool = true;
			var inputDateTime = new DateTime(2024, 12, 4, 10, 30, 0);
			var outputString = "OutputValue";
			var outputInt = 100;

			_dataProviderMock
				.MockExecuteProcess(processSchemaName)
				.HasInputParameter("StringParam", inputString)
				.HasInputParameter("IntParam", inputInt)
				.HasInputParameter("DecimalParam", inputDecimal)
				.HasInputParameter("GuidParam", inputGuid)
				.HasInputParameter("BoolParam", inputBool)
				.HasInputParameter("DateTimeParam", inputDateTime)
				.Returns(new Dictionary<string, object>
				{
					{ "OutputStringParam", outputString },
					{ "OutputIntParam", outputInt }
				});

			var model = new TestSimpleParametersProcessModel
			{
				StringParam = inputString,
				IntParam = inputInt,
				DecimalParam = inputDecimal,
				GuidParam = inputGuid,
				BoolParam = inputBool,
				DateTimeParam = inputDateTime
			};

			// Act
			var response = _appProcessContext.RunProcess(model);

			// Assert
			Assert.IsTrue(response.Success);
			Assert.IsNull(response.ErrorMessage);
			Assert.AreEqual(outputString, response.Result.OutputStringParam);
			Assert.AreEqual(outputInt, response.Result.OutputIntParam);
		}

		[Test]
		public void MockExecuteProcess_WithComplexParameters_ShouldReturnExpectedValues()
		{
			// Arrange
			var processSchemaName = "TestComplexParametersProcess";
			var inputParams = new List<CustomParameter>
			{
				new CustomParameter { Key = "Key1", Value = 10.5m, Position = 1 },
				new CustomParameter { Key = "Key2", Value = 20.5m, Position = 2 }
			};

			var expectedOutput = new List<CustomParameter>
			{
				new CustomParameter { Key = "Key1_processed", Value = 21.0m, Position = 1 },
				new CustomParameter { Key = "Key2_processed", Value = 41.0m, Position = 2 }
			};

			_dataProviderMock
				.MockExecuteProcess(processSchemaName)
				.HasInputParameter("InputParams", inputParams)
				.Returns("OutputParams", expectedOutput);

			var model = new TestComplexParametersProcessModel
			{
				InputParams = inputParams
			};

			// Act
			var response = _appProcessContext.RunProcess(model);

			// Assert
			Assert.IsTrue(response.Success);
			Assert.IsNull(response.ErrorMessage);
			Assert.IsNotNull(response.Result.OutputParams);
			Assert.AreEqual(2, response.Result.OutputParams.Count);
			Assert.AreEqual("Key1_processed", response.Result.OutputParams[0].Key);
			Assert.AreEqual(21.0m, response.Result.OutputParams[0].Value);
			Assert.AreEqual(1, response.Result.OutputParams[0].Position);
			Assert.AreEqual("Key2_processed", response.Result.OutputParams[1].Key);
			Assert.AreEqual(41.0m, response.Result.OutputParams[1].Value);
			Assert.AreEqual(2, response.Result.OutputParams[1].Position);
		}

		[Test]
		public void MockExecuteProcess_ReceivedCount_ShouldTrackInvocations()
		{
			// Arrange
			var processSchemaName = "TestSimpleParametersProcess";
			var inputString = "TestValue";
			var inputInt = 42;

			var mock = _dataProviderMock
				.MockExecuteProcess(processSchemaName)
				.HasInputParameter("StringParam", inputString)
				.HasInputParameter("IntParam", inputInt)
				.Returns(new Dictionary<string, object>
				{
					{ "OutputStringParam", "Result" }
				});

			var model = new TestSimpleParametersProcessModel
			{
				StringParam = inputString,
				IntParam = inputInt
			};

			// Act
			Assert.AreEqual(0, mock.ReceivedCount);
			_appProcessContext.RunProcess(model);
			Assert.AreEqual(1, mock.ReceivedCount);
			_appProcessContext.RunProcess(model);
			Assert.AreEqual(2, mock.ReceivedCount);
			_appProcessContext.RunProcess(model);

			// Assert
			Assert.AreEqual(3, mock.ReceivedCount);
		}

		[Test]
		public void MockExecuteProcess_WithErrorResponse_ShouldReturnError()
		{
			// Arrange
			var processSchemaName = "TestErrorProcess";
			var errorMessage = "Process execution failed";

			_dataProviderMock
				.MockExecuteProcess(processSchemaName)
				.Returns(false, errorMessage);

			var model = new TestErrorProcessModel
			{
				InputParam = "TestValue"
			};

			// Act
			var response = _appProcessContext.RunProcess(model);

			// Assert
			Assert.IsFalse(response.Success);
			Assert.AreEqual(errorMessage, response.ErrorMessage);
		}

		[Test]
		public void MockExecuteProcess_WithReceiveHandler_ShouldInvokeCallback()
		{
			// Arrange
			var processSchemaName = "TestSimpleParametersProcess";
			var inputString = "TestValue";
			var callbackInvoked = false;
			Dictionary<string, string> receivedParams = null;

			_dataProviderMock
				.MockExecuteProcess(processSchemaName)
				.HasInputParameter("StringParam", inputString)
				.Returns(new Dictionary<string, object> { { "OutputStringParam", "Result" } })
				.ReceiveHandler(mock =>
				{
					callbackInvoked = true;
					receivedParams = mock.GetReceivedInputParameters();
				});

			var model = new TestSimpleParametersProcessModel
			{
				StringParam = inputString
			};

			// Act
			Assert.IsFalse(callbackInvoked);
			var response = _appProcessContext.RunProcess(model);

			// Assert
			Assert.IsTrue(callbackInvoked);
			Assert.IsNotNull(receivedParams);
			Assert.IsTrue(receivedParams.ContainsKey("StringParam"));
		}

		[Test]
		public void MockExecuteProcess_WhenDisabled_ShouldNotMatch()
		{
			// Arrange
			var processSchemaName = "TestSimpleParametersProcess";
			var inputString = "TestValue";

			var mock = _dataProviderMock
				.MockExecuteProcess(processSchemaName)
				.HasInputParameter("StringParam", inputString)
				.Returns(new Dictionary<string, object> { { "OutputStringParam", "Result" } });

			mock.Enabled = false;

			var model = new TestSimpleParametersProcessModel
			{
				StringParam = inputString
			};

			// Act
			var response = _appProcessContext.RunProcess(model);

			// Assert
			Assert.IsFalse(response.Success);
			Assert.IsNotNull(response.ErrorMessage);
			Assert.IsTrue(response.ErrorMessage.Contains("No mock found"));
			Assert.AreEqual(0, mock.ReceivedCount);
		}

		[Test]
		public void MockExecuteProcess_WithBidirectionalParameters_ShouldHandleInputAndOutput()
		{
			// Arrange
			var processSchemaName = "TestBidirectionalProcess";
			var inputString = "InitialValue";
			var inputInt = 10;
			var inputList = new List<CustomParameter>
			{
				new CustomParameter { Key = "Input1", Value = 5.0m, Position = 1 }
			};

			var outputString = "ModifiedValue";
			var outputInt = 20;
			var outputList = new List<CustomParameter>
			{
				new CustomParameter { Key = "Output1", Value = 10.0m, Position = 1 }
			};

			_dataProviderMock
				.MockExecuteProcess(processSchemaName)
				.HasInputParameter("BidirectionalString", inputString)
				.HasInputParameter("BidirectionalInt", inputInt)
				.HasInputParameter("BidirectionalList", inputList)
				.Returns(new Dictionary<string, object>
				{
					{ "BidirectionalString", outputString },
					{ "BidirectionalInt", outputInt },
					{ "BidirectionalList", outputList }
				});

			var model = new TestBidirectionalProcessModel
			{
				BidirectionalString = inputString,
				BidirectionalInt = inputInt,
				BidirectionalList = inputList
			};

			// Act
			var response = _appProcessContext.RunProcess(model);

			// Assert
			Assert.IsTrue(response.Success);
			Assert.AreEqual(outputString, response.Result.BidirectionalString);
			Assert.AreEqual(outputInt, response.Result.BidirectionalInt);
			Assert.IsNotNull(response.Result.BidirectionalList);
			Assert.AreEqual(1, response.Result.BidirectionalList.Count);
			Assert.AreEqual("Output1", response.Result.BidirectionalList[0].Key);
			Assert.AreEqual(10.0m, response.Result.BidirectionalList[0].Value);
		}
	}
}
