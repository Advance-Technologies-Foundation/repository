namespace ATF.Repository.Mapping
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using ATF.Repository.Attributes;
    using Terrasoft.Common;

    internal class ModelMapper
	{
		private IDictionary<Type, List<ModelItem>> _cashedItems;
		private IDictionary<Type, Func<PropertyInfo, ModelItem>> _typeConverters;

		public ModelMapper() {
			_cashedItems = new Dictionary<Type, List<ModelItem>>();
			_typeConverters = GetTypeConverters();
		}

		public List<ModelItem> GetModelItems(Type modelType) {
			if (!_cashedItems.ContainsKey(modelType)) {
				_cashedItems.Add(modelType, FetchModelItemsFromModelType(modelType));
			}
			return _cashedItems[modelType];
		}

		public List<ModelItem> GetProperties(Type modelType) {
			var modelItems = GetModelItems(modelType);
			return modelItems.Where(x => x.PropertyType == ModelItemType.Column).ToList();
		}

		[Obsolete("Will be removed in 1.3.0")]
		public List<ModelItem> GetReferences(Type modelType) {
			var modelItems = GetModelItems(modelType);
			return modelItems.Where(x => x.PropertyType == ModelItemType.Reference).ToList();
		}

		public List<ModelItem> GetLookups(Type modelType) {
			var modelItems = GetModelItems(modelType);
			return modelItems.Where(x => x.PropertyType == ModelItemType.Lookup).ToList();
		}

		public List<ModelItem> GetDetails(Type modelType) {
			var modelItems = GetModelItems(modelType);
			return modelItems.Where(x => x.PropertyType == ModelItemType.Detail).ToList();
		}

		protected IDictionary<Type, Func<PropertyInfo, ModelItem>> GetTypeConverters() {
			return new Dictionary<Type, Func<PropertyInfo, ModelItem>>() {
				{ typeof(SchemaPropertyAttribute), SchemaPropertyTypeConverter },
				{ typeof(LookupPropertyAttribute), LookupPropertyTypeConverter },
				{ typeof(ReferencePropertyAttribute), ReferencePropertyTypeConverter },
				{ typeof(DetailPropertyAttribute), DetailPropertyTypeConverter },
			};
		}

		protected ModelItem CreatrModelItem(PropertyInfo propertyInfo, ModelItemType modelItemType) {
			return new ModelItem() {
				PropertyName = propertyInfo.Name,
				DataValueType = propertyInfo.PropertyType,
				PropertyType = modelItemType,
				IsLazy = propertyInfo.GetGetMethod().IsVirtual,
				PropertyInfo = propertyInfo
			};
		}

		protected ModelItem SchemaPropertyTypeConverter(PropertyInfo propertyInfo) {
			ModelItem modelItem = null;
			var attribute = propertyInfo.GetCustomAttribute(typeof(SchemaPropertyAttribute)) as SchemaPropertyAttribute;
			if (attribute != null) {
				modelItem = CreatrModelItem(propertyInfo, ModelItemType.Column);
				modelItem.EntityColumnName = attribute.Name;
			}
			return modelItem;
		}

		protected ModelItem LookupPropertyTypeConverter(PropertyInfo propertyInfo) {
			ModelItem modelItem = null;
			var attribute = propertyInfo.GetCustomAttribute(typeof(LookupPropertyAttribute)) as LookupPropertyAttribute;
			if (attribute != null) {
				modelItem = CreatrModelItem(propertyInfo, ModelItemType.Lookup);
				modelItem.EntityColumnName = attribute.Name;
			}
			return modelItem;
		}

		[Obsolete("Will be removed in 1.3.0")]
		protected ModelItem ReferencePropertyTypeConverter(PropertyInfo propertyInfo) {
			ModelItem modelItem = null;
			var attribute = propertyInfo.GetCustomAttribute(typeof(ReferencePropertyAttribute)) as ReferencePropertyAttribute;
			if (attribute != null) {
				modelItem = CreatrModelItem(propertyInfo, ModelItemType.Reference);
				modelItem.EntityColumnName = attribute.Name;
			}
			return modelItem;
		}

		protected ModelItem DetailPropertyTypeConverter(PropertyInfo propertyInfo) {
			ModelItem modelItem = null;
			var attribute = propertyInfo.GetCustomAttribute(typeof(DetailPropertyAttribute)) as DetailPropertyAttribute;
			if (attribute != null) {
				Type type = propertyInfo.PropertyType.GenericTypeArguments?.FirstOrDefault();
				modelItem = CreatrModelItem(propertyInfo, ModelItemType.Detail);
				modelItem.DataValueType = type;
				modelItem.MasterModelPropertyName = attribute.MasterFilterPropertyName;
				modelItem.DetailModelPropertyName = attribute.DetailFilterPropertyName;
			}
			return modelItem;
		}

		protected List<ModelItem> FetchModelItemsFromModelType(Type modelType) {
			var response = new List<ModelItem>();
			modelType.GetProperties().ForEach(x => {
				var modelItem = GetModelItem(x);
				if (modelItem != null) {
					response.Add(modelItem);
				}
			});
			return response;
		}

		protected ModelItem GetModelItem(PropertyInfo propertyInfo) {
			ModelItem modelItem = null;
			foreach(var item in _typeConverters) {
				if (modelItem != null) {
					continue;
				}
				modelItem = item.Value.Invoke(propertyInfo);
			}
			return modelItem;
		}

	}
}
