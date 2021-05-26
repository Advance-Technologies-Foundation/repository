using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ATF.Repository.Attributes;
using ATF.Repository.Mapping;

namespace ATF.Repository
{
	internal static class ModelUtilities
	{
		internal static string GetSchemaName(this BaseModel model)
		{
			MemberInfo type = model.GetType();
			return GetSchemaName(type);
		}

		internal static Dictionary<string, object> GetModelPropertyValues(this BaseModel model)
		{
			var response = new Dictionary<string, object>();
			ModelMapper.GetProperties(model.GetType()).ForEach(x => {
				response.Add(x.PropertyName, x.PropertyInfo.GetValue(model));
			});
			return response;
		}

		internal static string GetSchemaName(MemberInfo type)
		{
			string name = string.Empty;
			if (Attribute.IsDefined(type, typeof(SchemaAttribute)) &&
			    (type.GetCustomAttribute(typeof(SchemaAttribute)) as SchemaAttribute) != null) {
				SchemaAttribute attribute = type.GetCustomAttribute(typeof(SchemaAttribute)) as SchemaAttribute;
				name = attribute?.Name;
			}
			return name;
		}

		internal static string GetColumnPath(Type modelType, List<string> chain) {
			var columnPathList = new List<string>();
			var columnSourceType = modelType;
			foreach (var item in chain) {
				var modelItems = ModelMapper.GetModelItems(columnSourceType);
				var modelItem = modelItems.First(x => x.PropertyName == item);
				columnPathList.Add(modelItem.EntityColumnName);
				columnSourceType = modelItem.DataValueType;
			}

			return string.Join(".", columnPathList);
		}
	}
}
