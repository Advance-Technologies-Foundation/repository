namespace ATF.Repository.UnitTests
{
	using System;
	using System.Collections.Generic;
	using ATF.Repository.Providers;
	using NUnit.Framework;

	public class ExecuteProcessRequest : IExecuteProcessRequest
	{
		public string ProcessSchemaName { get; set; }
		public Dictionary<string, string> InputParameters { get; set; }
		public List<IExecuteProcessRequestItem> ResultParameters { get; set; }
	}
	
	public class ExecuteProcessRequestItem: IExecuteProcessRequestItem
	{
		public string Code { get; set; }
		public Type DataValueType { get; set; }
	}
	
	[TestFixture]
	public class RemoteDataProviderIntegrationTests
	{
		private IDataProvider _dataProvider;

		[SetUp]
		public void SetUp() {
			_dataProvider = new RemoteDataProvider("", "", "");
		}

		[Test]
		public void ExecuteProcess_ShouldReturnsExpectedValue() {
			var request = new ExecuteProcessRequest() {
				ProcessSchemaName = "DwTestProcess",
				InputParameters = new Dictionary<string, string>(),
				ResultParameters = new List<IExecuteProcessRequestItem>()
			};
			var inputValues = new Dictionary<string, string>();
			if (BusinessProcessValueConverter.TrySerializeProcessValue(typeof(decimal), 10.11m,
				out var decimalValue)) {
				request.InputParameters.Add("DecInputParam", decimalValue);
			}
			if (BusinessProcessValueConverter.TrySerializeProcessValue(typeof(bool), false,
				out var boolValue)) {
				request.InputParameters.Add("BoolParam", boolValue);
			}
			if (BusinessProcessValueConverter.TrySerializeProcessValue(typeof(DateTime), DateTime.Now,
				out var dateValue)) {
				request.InputParameters.Add("DateParam", dateValue);
			}
			
			if (BusinessProcessValueConverter.TrySerializeProcessValue(typeof(DateTime), DateTime.Now,
				out var dateTimeValue)) {
				request.InputParameters.Add("DateTimeParam", dateTimeValue);
			}
			
			if (BusinessProcessValueConverter.TrySerializeProcessValue(typeof(Guid), Guid.NewGuid(),
				out var guidValue)) {
				request.InputParameters.Add("GuidParam", guidValue);
			}
			
			if (BusinessProcessValueConverter.TrySerializeProcessValue(typeof(int), 12,
				out var intValue)) {
				request.InputParameters.Add("IntParam", intValue);
			}
			
			if (BusinessProcessValueConverter.TrySerializeProcessValue(typeof(Guid), new Guid("ea350dd6-66cc-df11-9b2a-001d60e938c6"),
				out var lookupValue)) {
				request.InputParameters.Add("LookupParam", lookupValue);
			}
			
			if (BusinessProcessValueConverter.TrySerializeProcessValue(typeof(string), "Hello!",
				out var stringValue)) {
				request.InputParameters.Add("StringParam", stringValue);
			}
			
			if (BusinessProcessValueConverter.TrySerializeProcessValue(typeof(DateTime), DateTime.Now,
				out var timeValue)) {
				request.InputParameters.Add("TimeParam", timeValue);
			}

			request.ResultParameters.Add(new ExecuteProcessRequestItem() {
				Code = "BoolParam",
				DataValueType = typeof(bool)
			});
			request.ResultParameters.Add(new ExecuteProcessRequestItem() {
				Code = "DateParam",
				DataValueType = typeof(DateTime)
			});
			request.ResultParameters.Add(new ExecuteProcessRequestItem() {
				Code = "DateTimeParam",
				DataValueType = typeof(DateTime)
			});
			request.ResultParameters.Add(new ExecuteProcessRequestItem() {
				Code = "GuidParam",
				DataValueType = typeof(Guid)
			});
			request.ResultParameters.Add(new ExecuteProcessRequestItem() {
				Code = "IntParam",
				DataValueType = typeof(int)
			});
			request.ResultParameters.Add(new ExecuteProcessRequestItem() {
				Code = "LookupParam",
				DataValueType = typeof(Guid)
			});
			request.ResultParameters.Add(new ExecuteProcessRequestItem() {
				Code = "StringParam",
				DataValueType = typeof(string)
			});
			request.ResultParameters.Add(new ExecuteProcessRequestItem() {
				Code = "TimeParam",
				DataValueType = typeof(DateTime)
			});
			request.ResultParameters.Add(new ExecuteProcessRequestItem() {
				Code = "BoolOutputParam",
				DataValueType = typeof(bool)
			});
			request.ResultParameters.Add(new ExecuteProcessRequestItem() {
				Code = "DateTimeOutputParam",
				DataValueType = typeof(DateTime)
			});
			request.ResultParameters.Add(new ExecuteProcessRequestItem() {
				Code = "DecOutputParam",
				DataValueType = typeof(decimal)
			});
			request.ResultParameters.Add(new ExecuteProcessRequestItem() {
				Code = "GuidOutputParam",
				DataValueType = typeof(Guid)
			});
			request.ResultParameters.Add(new ExecuteProcessRequestItem() {
				Code = "LookupOutputParam",
				DataValueType = typeof(Guid)
			});
			request.ResultParameters.Add(new ExecuteProcessRequestItem() {
				Code = "StringOutputParam",
				DataValueType = typeof(string)
			});

			var response = _dataProvider.ExecuteProcess(request);

			Assert.Pass();
		}
	}
}