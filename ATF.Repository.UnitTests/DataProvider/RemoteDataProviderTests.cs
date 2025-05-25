using System;
using System.Collections.Generic;
using System.Linq;
using ATF.Repository.Attributes;
using ATF.Repository.Providers;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;
using Terrasoft.Core.ServiceModelContract;

namespace ATF.Repository.UnitTests.DataProvider {
	
	[Category("BusinessProcess")]
	[TestFixture]
	public class RemoteDataProviderTests {

		private RemoteDataProvider _sut;
		private readonly ICreatioClientAdapter _adapterMock = Substitute.For<ICreatioClientAdapter>();
		private const string ApplicationUrlFake = "https://fake.creatio.com";
		private const string UserNameFake = "Fake_Supervisor";
		private const string PasswordFake = "Fake_Supervisor";

		[SetUp]
		public void Setup() {
			_sut = new RemoteDataProvider(ApplicationUrlFake,UserNameFake,PasswordFake) {
				CreatioClientAdapter = _adapterMock
			};
		}
		
		[Test]
		public void RunProcess_Builds_Correct_RequestFromModel() {
			FakeModel model = new FakeModel {
				IOne = "Input value one",
				ParamTwo = "Input value two",
				ParamThree = "Input value three",
				ParamFour = null,
			};
			const string creatioResponse = "whatever json i want";
			
			string calledPayload = string.Empty; 
			string calledUrl = string.Empty;
			_adapterMock
				.ExecutePostRequest(Arg.Any<string>(), Arg.Any<string>(),Arg.Any<int>())
				.Returns(creatioResponse)
				.AndDoes(c=> {
					calledUrl = c.ArgAt<string>(0);
					calledPayload = c.ArgAt<string>(1);
				});
			
			_ = _sut.RunProcess(model);
			
			ValidateUrl(calledUrl).Should().BeTrue("Expect to call correct url");
			ValidatePayloadProperty(calledPayload, "$.schemaName", "Fake_ProcessName");
			ValidatePayloadProperty(calledPayload, "$.collectExecutionData", true);
			
			ValidateParameterValuesCount(calledPayload, 3);
			ValidateResultParameterNames(calledPayload, new [] {"I_Three", "I_Four", "P_Two"});
			ValidatePayloadProperty(calledPayload, "$.parameterValues[0].name", "I_One");
			ValidatePayloadProperty(calledPayload, "$.parameterValues[0].value", model.IOne);
			
			ValidatePayloadProperty(calledPayload, "$.parameterValues[1].name", "I_Two");
			ValidatePayloadProperty(calledPayload, "$.parameterValues[1].value", model.ParamTwo);
			
			ValidatePayloadProperty(calledPayload, "$.parameterValues[2].name", "I_Three");
			ValidatePayloadProperty(calledPayload, "$.parameterValues[2].value", model.ParamThree);
			
		}

		[Test]
		public void RunProcess_Builds_Correct_RequestFromModel_WhenModelHasNoInput() {
			FakeModelWithoutInput model = new FakeModelWithoutInput();
			const string creatioResponse = "whatever json i want";
			
			string calledPayload = string.Empty; 
			string calledUrl = string.Empty;
			_adapterMock
				.ExecutePostRequest(Arg.Any<string>(), Arg.Any<string>(),Arg.Any<int>())
				.Returns(creatioResponse)
				.AndDoes(c=> {
					calledUrl = c.ArgAt<string>(0);
					calledPayload = c.ArgAt<string>(1);
				});
			
			_ = _sut.RunProcess(model);
			
			ValidateUrl(calledUrl).Should().BeTrue("Expect to call correct url");
			ValidatePayloadProperty(calledPayload, "$.schemaName", "Fake_ProcessName");
			ValidatePayloadProperty(calledPayload, "$.collectExecutionData", true);
			
			ValidateParameterValuesCount(calledPayload, 0);
		}
		
		[Test]
		public void RunProcess_Builds_Correct_RequestFromModel_WhenModelHasNoOutput() {
			FakeModelWithoutOutput model = new FakeModelWithoutOutput {
				InputOne = "Input value one"
			};
			
			const string creatioResponse = "whatever json i want";
			string calledPayload = string.Empty; 
			string calledUrl = string.Empty;
			_adapterMock
				.ExecutePostRequest(Arg.Any<string>(), Arg.Any<string>(),Arg.Any<int>())
				.Returns(creatioResponse)
				.AndDoes(c=> {
					calledUrl = c.ArgAt<string>(0);
					calledPayload = c.ArgAt<string>(1);
				});
			
			_ = _sut.RunProcess(model);
			ValidateUrl(calledUrl).Should().BeTrue("Expect to call correct url");
			ValidateResultParameterNames(calledPayload, null);
		}
		
		
		[Test]
		public void RunProcess() {
			FakeModelWithoutInput model = new FakeModelWithoutInput();
			RunProcessResponse creatioResponse = new RunProcessResponse {
				ProcessId = Guid.NewGuid(),
				ProcessStatus = 1,
				ResultParameterValues = new Dictionary<string, object> {
					{ "I_Three", "Output value three" },
					{ "I_Four", "Output value four" },
					{ "P_Two", "Output value two" }
				}
			};
		
			string calledPayload = string.Empty; 
			string calledUrl = string.Empty;
			_adapterMock
				.ExecutePostRequest(Arg.Any<string>(), Arg.Any<string>(),Arg.Any<int>())
				.Returns(JsonConvert.SerializeObject(creatioResponse))
				.AndDoes(c=> {
					calledUrl = c.ArgAt<string>(0);
					calledPayload = c.ArgAt<string>(1);
				});
			
			RunProcessResponseWrapper<FakeModelWithoutInput> x  = _sut.RunProcess(model);
			
			x.ResultModel.PTwo.Should().BeNull("Model should not have output values before process execution");
			x.ResultModel.Should().Be("Output value two");
			model.PTwo.Should().Be(x.ResultModel.PTwo, "Model should be updated with output values from the process");
		}
		
		private static bool ValidateUrl(string input) => 
			input.EndsWith("ServiceModel/ProcessEngineService.svc/RunProcess");
		private static void ValidatePayloadProperty(string input, string jsonPath, object expectedValue){
			JObject obj = JObject.Parse(input);
			JToken actualValue = obj.SelectToken(jsonPath);
			actualValue.Should().NotBeNull();
			actualValue!.ToObject<object>().Should()
						.BeEquivalentTo(expectedValue, "Actual value: {0} should be be equivalent to expected: {1}", actualValue, expectedValue);
			
		} 
		private static void ValidateParameterValuesCount(string input, int expectedCount) {
			JObject obj = JObject.Parse(input);
			JToken actualValue = obj.SelectToken("$.parameterValues");
			if(expectedCount == 0) {
				actualValue.Should().BeNullOrEmpty("Expected parameterValues to be null when there are no input parameters");
				return;
			}
			actualValue.Should().HaveCount(expectedCount);
		}
		private static void ValidateResultParameterNames(string input, IEnumerable<string> expectedResultParameterNames) {
			JObject obj = JObject.Parse(input);
			JToken actualValue = obj.SelectToken("$.resultParameterNames");
			if(expectedResultParameterNames == null || !expectedResultParameterNames.Any()) {
				actualValue.Should().BeNullOrEmpty("Expected result parameter names to be null when there are no output parameters");
				return;
			}
			string[] actualValueOrderd = actualValue.ToObject<string[]>().Order().ToArray();
			expectedResultParameterNames.Order().ToArray()
				.Should().BeEquivalentTo(actualValueOrderd, "Expected result parameter names to match the actual values");
		}
		
	}
	
	
	[Schema("Fake_ProcessName")]
	public class FakeModel: BaseBpModel {

		[ProcessParameter("I_One", ProcessParameterDirection.Input)]
		public string IOne { get; set; }

		[ProcessParameter("I_Two", ProcessParameterDirection.Input)]
		public string ParamTwo { get; set; }

		[ProcessParameter("I_Three", ProcessParameterDirection.Bidirectional)]
		public string ParamThree { get; set; }

		[ProcessParameter("I_Four", ProcessParameterDirection.Bidirectional)]
		public string ParamFour { get; set; }
		
		[ProcessParameter("P_Two", ProcessParameterDirection.Output)]
		public string PTwo { get; set; }

	}
	
	[Schema("Fake_ProcessName")]
	public class FakeModelWithoutInput: BaseBpModel {

		[ProcessParameter("P_Two", ProcessParameterDirection.Output)]
		public string PTwo { get; set; }

	}
	
	[Schema("Fake_ProcessName")]
	public class FakeModelWithoutOutput: BaseBpModel {

		[ProcessParameter("InputOne", ProcessParameterDirection.Input)]
		public string InputOne { get; set; }

	}
	
}
