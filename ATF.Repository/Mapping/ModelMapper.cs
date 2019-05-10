namespace ATF.Repository.Mapping
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using ATF.Repository.Attributes;

	internal class ModelMapper
	{
		public List<ModelParameter> GetParameters(Type modelType) {
			var response = new List<ModelParameter>();

			List<PropertyInfo> properties = modelType.GetProperties().Where(
				prop => Attribute.IsDefined(prop, typeof(SchemaPropertyAttribute))).ToList();
			foreach (var property in properties) {
				var attribute = property.GetCustomAttribute(typeof(SchemaPropertyAttribute)) as SchemaPropertyAttribute;
				if (attribute != null) {
					response.Add(new ModelParameter() {
						Name = property.Name,
						EntitySchemaColumnName = attribute.Name,
						Type = property.PropertyType
					});
				}
			}

			return response;
		}

		public List<ModelReference> GetReferences(Type modelType) {
			var response = new List<ModelReference>();

			List<PropertyInfo> properties = modelType.GetProperties().Where(
				prop => Attribute.IsDefined(prop, typeof(ReferencePropertyAttribute))).ToList();
			foreach (var property in properties) {
				var attribute = property.GetCustomAttribute(typeof(ReferencePropertyAttribute)) as ReferencePropertyAttribute;
				if (attribute != null) {
					response.Add(new ModelReference() {
						ValuePropertyName = attribute.Name,
						Name = property.Name,
						Type = property.PropertyType,
						IsLazyLoad = property.GetGetMethod().IsVirtual
					});
				}
			}

			return response;
		}

		public List<ModelDetail> GetDetails(Type modelType) {
			var response = new List<ModelDetail>();

			List<PropertyInfo> properties = modelType.GetProperties().Where(
				prop => Attribute.IsDefined(prop, typeof(DetailPropertyAttribute))).ToList();
			foreach (var property in properties) {
				var attribute = property.GetCustomAttribute(typeof(DetailPropertyAttribute)) as DetailPropertyAttribute;
				Type type = property.PropertyType.GenericTypeArguments?.FirstOrDefault();
				if (attribute != null) {
					response.Add(new ModelDetail() {
						Name = property.Name,
						MasterFilterPropertyName = attribute.MasterFilterPropertyName,
						DetailFilterPropertyName = attribute.DetailFilterPropertyName,
						Type = type,
						IsLazyLoad = property.GetGetMethod().IsVirtual
					});
				}
			}

			return response;
		}
	}
}
