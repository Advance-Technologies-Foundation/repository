namespace ATF.Repository.Mock.UnitTests.Models
{
	using ATF.Repository.Attributes;

	[Schema("SysAdminUnit")]
	public class SysAdminUnit: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}
