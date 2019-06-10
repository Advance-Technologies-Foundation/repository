namespace ATF.Repository
{
	using ATF.Repository.Attributes;
	using System;
	using System.Collections.Generic;
	using Terrasoft.Core;
	using Terrasoft.Core.Entities;

	public abstract class BaseModel
	{
		public UserConnection UserConnection { protected get; set; }

		[SchemaProperty("Id")]
		public Guid Id { get; internal set; }

		private IDictionary<string, object> _lazyValues;
		internal IDictionary<string, object> LazyValues => _lazyValues ?? (_lazyValues = new Dictionary<string, object>());

		private IDictionary<string, object> _initValues;
		internal IDictionary<string, object> InitValues => _initValues ?? (_initValues = new Dictionary<string, object>());

		internal Repository Repository { get; set; }

		internal Entity Entity { get; set; }

		public bool IsNew { get; internal set; }

		public bool IsMarkAsDeleted { get; internal set; }

	}
}
