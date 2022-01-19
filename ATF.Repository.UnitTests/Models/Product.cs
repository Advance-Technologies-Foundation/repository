namespace ATF.Repository.UnitTests.Models
{
	using System;
	using ATF.Repository.Attributes;

	[Schema("Product")]
	public class Product: BaseModel
	{
		[SchemaProperty("Name")]
		public string Name { get; set; }

		[LookupProperty("Owner")]
		public virtual Contact Owner { get; set; }

		[SchemaProperty("Unit")]
		public Guid UnitId { get; set; }

		[LookupProperty("Currency")]
		public virtual Currency Currency { get; set; }

		[SchemaProperty("Active")]
		public bool Active { get; set; }

		[LookupProperty("ProductSource")]
		public virtual ProductSource ProductSource { get; set; }

		[SchemaProperty("Price")]
		public decimal Price { get; set; }

		[SchemaProperty("IntegratedOn")]
		public DateTime IntegratedOn { get; set; }

		[SchemaProperty("Code")]
		public string Code { get; set; }

		[SchemaProperty("URL")]
		public string Url { get; set; }

		[LookupProperty("Category")]
		public virtual ProductCategory Category { get; set; }

		[LookupProperty("Type")]
		public virtual ProductType Type { get; set; }

		[SchemaProperty("IsTarget")]
		public bool IsTarget { get; set; }

		[SchemaProperty("StartDate")]
		public DateTime StartDate { get; set; }

		[LookupProperty("Kind")]
		public virtual ProductKind Kind { get; set; }

		[SchemaProperty("RevenueRecognitionMethod")]
		public Guid RevenueRecognitionMethodId { get; set; }

	}
}
