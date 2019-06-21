namespace ATF.Repository.Mapping
{
	using System;
    using System.Reflection;

    internal class ModelItem
	{
		/// <summary>
		/// Name of model property
		/// </summary>
		internal string PropertyName { get; set; }

		/// <summary>
		/// Type of model ptoperty value
		/// </summary>
		internal Type DataValueType { get; set; }

		/// <summary>
		/// Type of model ptoperty
		/// </summary>
		internal ModelItemType PropertyType { get; set; }

		/// <summary>
		/// Mapping entity column name
		/// </summary>
		internal string EntityColumnName { get; set; }

		/// <summary>
		/// Master model property name. Like Invoice.Id
		/// </summary>
		internal string MasterModelPropertyName { get; set; }

		/// <summary>
		/// Detail model property name. Like InvoiceProduct.InvoiceId
		/// </summary>
		internal string DetailModelPropertyName { get; set; }

		/// <summary>
		/// Lazy load flag
		/// </summary>
		internal bool IsLazy { get; set; }

		/// <summary>
		/// Property info.
		/// </summary>
		internal PropertyInfo PropertyInfo { get; set; }
	}
}
