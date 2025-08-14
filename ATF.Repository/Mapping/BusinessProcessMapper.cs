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
			if (!(propertyInfo.GetCustomAttribute(typeof(BusinessProcessParameterAttribute)) is BusinessProcessParameterAttribute attr)) {
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

		#endregion

	}

	#endregion
	
}