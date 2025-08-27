namespace ATF.Repository.Mock.UnitTests.Models
{
	using System;
	using ATF.Repository.Attributes;

	[BusinessProcess("CommonProcess")]
	public class CommonProcessModel: IBusinessProcess
	{
		[BusinessProcessParameter("InputGuid", BusinessProcessParameterDirection.Input)]
		public Guid InputGuid { get; set; }

		[BusinessProcessParameter("BiDateTime", BusinessProcessParameterDirection.Bidirectional)]
		public DateTime BiDateTime { get; set; }

		[BusinessProcessParameter("BiDecimal", BusinessProcessParameterDirection.Bidirectional)]
		public decimal BiDecimal { get; set; }

		[BusinessProcessParameter("OutputBoolean", BusinessProcessParameterDirection.Output)]
		public bool OutputBoolean { get; set; }

		[BusinessProcessParameter("OutputInt", BusinessProcessParameterDirection.Output)]
		public int OutputInteger { get; set; }
	}
}
