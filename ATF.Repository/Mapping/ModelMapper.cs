namespace ATF.Repository.Mapping
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using ATF.Repository.Attributes;
	using Terrasoft.Common;

	internal static class ModelMapper
	{
		private static readonly IDictionary<Type, List<ModelItem>> CashedItems = new Dictionary<Type, List<ModelItem>>();
		private static readonly IDictionary<Type, Func<PropertyInfo, ModelItem>> TypeConverters = GetTypeConverters();

		public static List<ModelItem> GetModelItems(Type modelType) {
			if (!CashedItems.ContainsKey(modelType)) {
				CashedItems.Add(modelType, FetchModelItemsFromModelType(modelType));
			}
			return CashedItems[modelType];
		}

		public static List<ModelItem> GetProperties(Type modelType) {
			var modelItems = GetModelItems(modelType);
			return modelItems.Where(x => x.PropertyType == ModelItemType.Column).ToList();
		}

		[Obsolete("Will be removed in 1.3.0")]
		public static List<ModelItem> GetReferences(Type modelType) {
			var modelItems = GetModelItems(modelType);
			return modelItems.Where(x => x.PropertyType == ModelItemType.Reference).ToList();
		}

		public static List<ModelItem> GetLookups(Type modelType) {
			var modelItems = GetModelItems(modelType);
			return modelItems.Where(x => x.PropertyType == ModelItemType.Lookup).ToList();
		}

		public static List<ModelItem> GetDetails(Type modelType) {
			var modelItems = GetModelItems(modelType);
			return modelItems.Where(x => x.PropertyType == ModelItemType.Detail).ToList();
		}

		private static IDictionary<Type, Func<PropertyInfo, ModelItem>> GetTypeConverters() {
			return new Dictionary<Type, Func<PropertyInfo, ModelItem>>() {
				{ typeof(SchemaPropertyAttribute), SchemaPropertyTypeConverter },
				{ typeof(LookupPropertyAttribute), LookupPropertyTypeConverter },
				{ typeof(DetailPropertyAttribute), DetailPropertyTypeConverter },
			};
		}

		private static ModelItem CreateModelItem(PropertyInfo propertyInfo, ModelItemType modelItemType) {
			return new ModelItem() {
				PropertyName = propertyInfo.Name,
				DataValueType = propertyInfo.PropertyType,
				PropertyType = modelItemType,
				IsLazy = propertyInfo.GetGetMethod().IsVirtual,
				PropertyInfo = propertyInfo
			};
		}

		private static ModelItem SchemaPropertyTypeConverter(PropertyInfo propertyInfo) {
			if (!(propertyInfo.GetCustomAttribute(typeof(SchemaPropertyAttribute)) is SchemaPropertyAttribute attr)) {
				return null;
			}
			var modelItem = CreateModelItem(propertyInfo, ModelItemType.Column);
			modelItem.EntityColumnName = attr.Name;
			return modelItem;
		}

		private static ModelItem LookupPropertyTypeConverter(PropertyInfo propertyInfo) {
			if (!(propertyInfo.GetCustomAttribute(typeof(LookupPropertyAttribute)) is LookupPropertyAttribute attr)) {
				return null;
			}
			var modelItem = CreateModelItem(propertyInfo, ModelItemType.Lookup);
			modelItem.EntityColumnName = attr.Name;
			return modelItem;
		}

		private static ModelItem DetailPropertyTypeConverter(PropertyInfo propertyInfo) {
			if (!(propertyInfo.GetCustomAttribute(typeof(DetailPropertyAttribute)) is DetailPropertyAttribute attr)) {
				return null;
			}
			var type = propertyInfo.PropertyType.GenericTypeArguments?.FirstOrDefault();
			var modelItem = CreateModelItem(propertyInfo, ModelItemType.Detail);
			modelItem.DataValueType = type;
			modelItem.MasterEntityColumnName = attr.MasterLinkPropertyName;
			modelItem.DetailLinkPropertyName = attr.DetailLinkPropertyName;
			return modelItem;
		}

		private static List<ModelItem> FetchModelItemsFromModelType(Type modelType) {
			var response = new List<ModelItem>();
			modelType.GetProperties().ForEach(x => {
				var modelItem = GetModelItem(x);
				if (modelItem != null) {
					response.Add(modelItem);
				}
			});
			return response;
		}

		private static ModelItem GetModelItem(PropertyInfo propertyInfo) {
			ModelItem modelItem = null;
			foreach (var item in TypeConverters) {
				if (modelItem != null) {
					continue;
				}
				modelItem = item.Value.Invoke(propertyInfo);
			}
			return modelItem;
		}

	}
}
