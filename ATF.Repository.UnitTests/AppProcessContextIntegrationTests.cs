namespace ATF.Repository.UnitTests
{
	using System;
	using ATF.Repository.Attributes;
	using ATF.Repository.Providers;
	using NUnit.Framework;

	[BusinessProcess("DwTestProcess")]
	public class DwTestProcessModel: IBusinessProcess
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
	public class AppProcessContextIntegrationTests
	{
		private IDataProvider _dataProvider;
		private IAppProcessContext _appProcessContext;

		[SetUp]
		public void SetUp() {
			_dataProvider = new RemoteDataProvider("", "", "");
			_appProcessContext = AppProcessContextFactory.GetAppProcessContext(_dataProvider);
		}

		[Test]
		public void RunProcess_ShouldReturnsExpectedValues() {
			var model = new DwTestProcessModel() {
				DecInputParam = 10.11m,
				BoolParamExt = false,
				DateParam = new DateTime(2025, 8, 14, 18, 16, 30),
				DateTimeParam = new DateTime(2025, 8, 14, 18, 16, 30),
				GuidParam = new Guid("ebc0b3d0-f30c-4302-956d-d0d67f14bed3"),
				IntParam = 12,
				LookupParameter = new Guid("ea350dd6-66cc-df11-9b2a-001d60e938c6"),
				StringParam = "Hello!",
				TimeParam = new DateTime(2025, 8, 14, 18, 16, 30)
			};
			var response = _appProcessContext.RunProcess(model);
			Assert.Pass();
		}
	}
}