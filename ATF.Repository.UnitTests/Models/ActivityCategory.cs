using ATF.Repository.Attributes;

namespace ATF.Repository.UnitTests.Models
{
	[Schema("ActivityCategory")]
	public class ActivityCategory: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }
	}
}