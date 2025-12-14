namespace ATF.Repository.UnitTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.Attributes;
	using ATF.Repository.Providers;
	using NUnit.Framework;

	[BusinessProcess("AtfTestCommonParametersProcess")]
	public class AtfTestCommonParametersProcessModel: IBusinessProcess
	{
		[BusinessProcessParameter("DecInputParam", BusinessProcessParameterDirection.Input)]
		public decimal DecInputParam { get; set; }
		
		[BusinessProcessParameter("BoolParam", BusinessProcessParameterDirection.Bidirectional)]
		public bool BoolParamExt { get; set; }
		
		[BusinessProcessParameter("DateParam", BusinessProcessParameterDirection.Bidirectional)]
		public DateTime DateParam { get; set; }
		
		[BusinessProcessParameter("DateTimeParam", BusinessProcessParameterDirection.Bidirectional)]
		public DateTime DateTimeParam { get; set; }
		
		[BusinessProcessParameter("GuidParam", BusinessProcessParameterDirection.Bidirectional)]
		public Guid GuidParam { get; set; }
		
		[BusinessProcessParameter("IntParam", BusinessProcessParameterDirection.Bidirectional)]
		public int IntParam { get; set; }
		
		[BusinessProcessParameter("LookupParam", BusinessProcessParameterDirection.Bidirectional)]
		public Guid LookupParameter { get; set; }
		
		[BusinessProcessParameter("StringParam", BusinessProcessParameterDirection.Bidirectional)]
		public string StringParam { get; set; }
		
		[BusinessProcessParameter("TimeParam", BusinessProcessParameterDirection.Bidirectional)]
		public DateTime TimeParam { get; set; }
		
		[BusinessProcessParameter("BoolOutputParam", BusinessProcessParameterDirection.Output)]
		public bool BoolOutputParam { get; set; }

		[BusinessProcessParameter("DateTimeOutputParam", BusinessProcessParameterDirection.Output)]
		public DateTime DateTimeOutputParam { get; set; }

		[BusinessProcessParameter("DecOutputParam", BusinessProcessParameterDirection.Output)]
		public decimal DecOutputParam { get; set; }

		[BusinessProcessParameter("GuidOutputParam", BusinessProcessParameterDirection.Output)]
		public Guid GuidOutputParam { get; set; }

		[BusinessProcessParameter("LookupOutputParam", BusinessProcessParameterDirection.Output)]
		public Guid LookupOutputParam { get; set; }

		[BusinessProcessParameter("StringOutputParam", BusinessProcessParameterDirection.Output)]
		public string StringOutputParam { get; set; }
	}

	public class AtfTestCustomObjectProcessParameter
	{
		[BusinessProcessParameter("Key", BusinessProcessParameterDirection.Bidirectional)]
		public string Key { get; set; }

		[BusinessProcessParameter("Value", BusinessProcessParameterDirection.Bidirectional)]
		public decimal Value { get; set; }

		[BusinessProcessParameter("Position", BusinessProcessParameterDirection.Bidirectional)]
		public int Position { get; set; }
	}

	[BusinessProcess("AtfTestCustomObjectProcess")]
	public class AtfTestCustomObjectProcessModel: IBusinessProcess
	{
		[BusinessProcessParameter("Code", BusinessProcessParameterDirection.Bidirectional)]
		public string Code { get; set; }
		
		[BusinessProcessParameter("Parameters", BusinessProcessParameterDirection.Input)]
		public List<AtfTestCustomObjectProcessParameter> Parameters { get; set; }
		
		[BusinessProcessParameter("ExportParameters", BusinessProcessParameterDirection.Output)]
		public List<AtfTestCustomObjectProcessParameter> ExportParameters { get; set; }
	}
	[TestFixture]
	public class AppProcessContextIntegrationTests : BaseIntegrationTests
	{
		private IDataProvider _dataProvider;
		private IAppProcessContext _appProcessContext;

		[SetUp]
		public void SetUp() {
			_dataProvider = GetIntegrationDataProvider();
			_appProcessContext = AppProcessContextFactory.GetAppProcessContext(_dataProvider);
		}

		[Test]
		public void RunProcessWithCommonProperties_ShouldReturnsExpectedValues() {
			var decInputParam = 10.11m;
			var boolParam = true;
			var dateParam = new DateTime(2025, 8, 14, 18, 16, 30);
			var dateTimeParam = new DateTime(2025, 8, 14, 18, 16, 30);
			var guidParam = Guid.NewGuid();
			var intParam = 12;
			var lookupParameter = new Guid("d44b9da2-53e6-df11-971b-001d60e938c6");
			var stringParam = "Hello!";
			var timeParam = new DateTime(2025, 8, 14, 18, 16, 30);
			var model = new AtfTestCommonParametersProcessModel() {
				DecInputParam = decInputParam,
				BoolParamExt = boolParam,
				DateParam = dateParam,
				DateTimeParam = dateTimeParam,
				GuidParam = guidParam,
				IntParam = intParam,
				LookupParameter = lookupParameter,
				StringParam = stringParam,
				TimeParam = timeParam
			};
			var response = _appProcessContext.RunProcess(model);
			Assert.IsNotNull(response);
			Assert.IsTrue(response.Success);
			Assert.IsNotNull(response.Result);
			Assert.AreEqual(boolParam, response.Result.BoolOutputParam);
			Assert.AreEqual(dateTimeParam, response.Result.DateTimeOutputParam.ToUniversalTime());
			Assert.AreEqual(decInputParam, response.Result.DecOutputParam);
			Assert.AreEqual(guidParam, response.Result.GuidOutputParam);
			Assert.AreEqual(lookupParameter, response.Result.LookupOutputParam);
			Assert.AreEqual(stringParam, response.Result.StringOutputParam);
			
			Assert.AreEqual(!boolParam, response.Result.BoolParamExt);
			Assert.AreEqual(dateParam.AddYears(1), response.Result.DateParam.ToUniversalTime());
			Assert.AreEqual(dateTimeParam.AddHours(1), response.Result.DateTimeParam.ToUniversalTime());
			Assert.IsTrue(response.Result.GuidParam != Guid.Empty && response.Result.GuidParam != guidParam);
			Assert.AreEqual(intParam + 1, response.Result.IntParam);
			Assert.AreEqual(new Guid("d44b9da2-53e6-df11-971b-001d60e938c6"), response.Result.LookupParameter);
			Assert.AreEqual(string.Concat(stringParam, "_fix"), response.Result.StringParam);
			Assert.AreEqual(timeParam.AddMinutes(10), response.Result.TimeParam.ToUniversalTime());
		}
		
		[Test]
		public void RunProcessWithCustomProperties_ShouldReturnsExpectedValues() {
			var model = new AtfTestCustomObjectProcessModel() {
				Code = "Code",
				Parameters = new List<AtfTestCustomObjectProcessParameter>() {
					new AtfTestCustomObjectProcessParameter() {
						Key = "Key 1",
						Position = 0,
						Value = 10.11m
					},
					new AtfTestCustomObjectProcessParameter() {
						Key = "Key 2",
						Position = 1,
						Value = 11.22m
					}
				}
			};

			var response = _appProcessContext.RunProcess(model);
			Assert.IsNotNull(response);
			Assert.IsTrue(response.Success, $"Process failed: {response.ErrorMessage}");
			Assert.IsNotNull(response.Result);
			Assert.IsNotNull(response.Result.ExportParameters, "ExportParameters is null");
			Assert.AreEqual(2, response.Result.ExportParameters.Count, $"Expected 2 export parameters, got {response.Result.ExportParameters.Count}");

			Assert.IsTrue(response.Result.ExportParameters.Any(x=>x.Key == "Key 1 fixed" && x.Value == 20.22m && x.Position == 0),
				"First parameter validation failed");
			Assert.IsTrue(response.Result.ExportParameters.Any(x=>x.Key == "Key 2 fixed" && x.Value == 22.44m && x.Position == 1),
				"Second parameter validation failed");
		}
	}
}