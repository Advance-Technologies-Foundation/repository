using System;
using System.Reflection;

namespace ATF.Repository.Mapping
{
	internal class BusinessProcessItem
	{
		public string PropertyName { get; set; }
		public string ProcessParameterName { get; set; }
		public BusinessProcessParameterDirection Direction { get; set; }
		public Type DataValueType { get; set; }
		public object Value { get; set; }
		public PropertyInfo PropertyInfo { get; set; }
	}
}