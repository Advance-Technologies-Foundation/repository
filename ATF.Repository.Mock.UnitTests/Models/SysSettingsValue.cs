namespace ATF.Repository.Mock.UnitTests.Models
{
	using System;
	using ATF.Repository.Attributes;

	[Schema("SysSettingsValue")]
	public class SysSettingsValue : BaseModel
	{
		[SchemaProperty("SysSettings")]
		public Guid SysSettingsId { get; set; }

		[LookupProperty("SysSettings")]
		public virtual SysSettings SysSettings { get; set; }

		[SchemaProperty("SysAdminUnit")]
		public Guid SysAdminUnitId { get; set; }

		[LookupProperty("SysAdminUnit")]
		public virtual SysAdminUnit SysAdminUnit { get; set; }

		[SchemaProperty("IsDef")]
		public bool IsDef { get; set; }

		[SchemaProperty("TextValue")]
		public string TextValue { get; set; }

		[SchemaProperty("IntegerValue")]
		public int IntegerValue { get; set; }

		[SchemaProperty("FloatValue")]
		public decimal FloatValue { get; set; }

		[SchemaProperty("BooleanValue")]
		public bool BooleanValue { get; set; }

		[SchemaProperty("DateTimeValue")]
		public DateTime DateTimeValue { get; set; }

		[SchemaProperty("GuidValue")]
		public Guid GuidValue { get; set; }

	}
}
