namespace ATF.Repository.Mapping
{
	using System.Collections.Generic;
	using System.Reflection;
	using ATF.Repository.Attributes;
	using Terrasoft.Common;

	#region Class: BusinessProcessMapper

	internal static class BusinessProcessMapper
	{
		#region Methods: Private

		private static BusinessProcessItem GetBusinessProcessItem<T>(PropertyInfo propertyInfo, T businessProcess) {
			if (!(propertyInfo.GetCustomAttribute(typeof(BusinessProcessParameterAttribute)) is
				BusinessProcessParameterAttribute attr)) {
				return null;
			}

			return new BusinessProcessItem() {
				PropertyName = propertyInfo.Name,
				DataValueType = propertyInfo.PropertyType,
				Direction = attr.Direction,
				ProcessParameterName = attr.Name,
				Value = propertyInfo.GetValue(businessProcess),
				PropertyInfo = propertyInfo
			};
		}

		#endregion

		#region Methods: Public

		public static List<BusinessProcessItem> GetParameters<T>(T businessProcess) where T : IBusinessProcess {
			ModelUtilities.ValidateBusinessProcess(businessProcess);
			var response = new List<BusinessProcessItem>();
			businessProcess.GetType().GetProperties().ForEach(x => {
				var businessProcessItem = GetBusinessProcessItem(x, businessProcess);
				if (businessProcessItem != null && !response.Contains(businessProcessItem)) {
					response.Add(businessProcessItem);
				}
			});
			return response;
		}

		/// <summary>
		/// Gets properties with BusinessProcessParameter attribute for a given type.
		/// </summary>
		/// <param name="type">The type to inspect.</param>
		/// <returns>List of PropertyInfo with BusinessProcessParameter attribute.</returns>
		public static List<PropertyInfo> GetBusinessProcessProperties(System.Type type) {
			var response = new List<PropertyInfo>();
			type.GetProperties().ForEach(propertyInfo => {
				if (propertyInfo.GetCustomAttribute(typeof(BusinessProcessParameterAttribute)) is
					BusinessProcessParameterAttribute) {
					response.Add(propertyInfo);
				}
			});
			return response;
		}

		/// <summary>
		/// Checks if type has any properties with BusinessProcessParameter attribute.
		/// </summary>
		/// <param name="type">The type to check.</param>
		/// <returns>True if type has BusinessProcessParameter properties.</returns>
		public static bool HasBusinessProcessParameters(System.Type type) {
			return GetBusinessProcessProperties(type).Count > 0;
		}

		#endregion

	}

	#endregion

}