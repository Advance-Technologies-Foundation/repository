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

		internal Entity InternalEntity { get; set; }

		internal ILazyModelPropertyManager LazyModelPropertyManager;

		public bool IsNew { get; internal set; }

		public bool IsMarkAsDeleted { get; internal set; }

		internal void SetLazyLookupKeyValue(string lookupPropertyName, Guid value) {
			if (value == Guid.Empty) {
				return;
			}
			var key = GetLazyLookupKey(lookupPropertyName);
			if (LazyValues.ContainsKey(key)) {
				LazyValues[key] = value;
			} else {
				LazyValues.Add(key, value);
			}
		}

		internal Guid GetLazyLookupKeyValue(string lookupPropertyName) {
			var key = GetLazyLookupKey(lookupPropertyName);
			return LazyValues.ContainsKey(key)
				? (Guid) LazyValues[key]
				: Guid.Empty;
		}

		internal string GetLazyLookupKey(string lookupPropertyName) {
			return $"Lookup::{lookupPropertyName}";
		}
	}
}
