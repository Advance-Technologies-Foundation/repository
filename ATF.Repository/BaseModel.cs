namespace ATF.Repository
{
	using ATF.Repository.Attributes;
	using System;
	using System.Collections.Generic;
	using Terrasoft.Core;

	public abstract class BaseModel
	{
		public UserConnection UserConnection { protected get; set; }

		[SchemaProperty("Id")]
		public Guid Id { get; internal set; }

		internal IDictionary<string, object> values { get; set; }

		internal Repository Repository { get; set; }

		public BaseModel() {
			values = new Dictionary<string, object>();
		}
	}
}
