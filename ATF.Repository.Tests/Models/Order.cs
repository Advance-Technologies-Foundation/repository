namespace ATF.Repository.Tests.Models
{
	using System;
	using System.Collections.Generic;
	using ATF.Repository.Attributes;

	[Schema(name: "Order")]
	public class Order : BaseModel
	{
		[SchemaProperty("Number")]
		public string Number { get; set; }

	}
}
