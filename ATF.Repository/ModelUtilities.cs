namespace ATF.Repository
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using ATF.Repository.Attributes;
	using ATF.Repository.Mapping;
	using Terrasoft.Common;

	internal static class ModelUtilities
	{
		internal static string GetSchemaName(this BaseModel model)
		{
			MemberInfo type = model.GetType();
			return GetSchemaName(type);
		}

		internal static bool IsModelType(Type type) {
			return type != null && (type == typeof(BaseModel) || type.IsSubclassOf(typeof(BaseModel)));
		}

		internal static Dictionary<string, object> GetModelPropertyValues(this BaseModel model)
		{
			var response = model.GetDirectModelPropertyValues();
			response.AddRange(model.GetLazyModelPropertyValues());
			return response;
		}

		internal static Dictionary<string, object> GetDirectModelPropertyValues(this BaseModel model)
		{
			var response = new Dictionary<string, object>();
			var modelType = model.GetType();
			ModelMapper.GetProperties(modelType).ForEach(x => {
				response.Add(x.EntityColumnName, x.PropertyInfo.GetValue(model));
			});
			return response;
		}

		internal static Dictionary<string, object> GetLazyModelPropertyValues(this BaseModel model) {
			var modelType = model.GetType();
			var existedDirectItems = ModelMapper.GetProperties(modelType).Select(x=>x.EntityColumnName).ToList();
			var response = new Dictionary<string, object>();
			ModelMapper.GetLookups(modelType).Where(x=>!existedDirectItems.Contains(x.EntityColumnName)).ForEach(x => {
				response.Add(x.EntityColumnName, model.GetLazyLookupKeyValue(x.PropertyName));
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
		
		internal static string GetBusinessProcessName(MemberInfo type)
		{
			string name = string.Empty;
			if (Attribute.IsDefined(type, typeof(BusinessProcessAttribute)) &&
				(type.GetCustomAttribute(typeof(BusinessProcessAttribute)) as BusinessProcessAttribute) != null) {
				BusinessProcessAttribute attribute = type.GetCustomAttribute(typeof(BusinessProcessAttribute)) as BusinessProcessAttribute;
				name = attribute?.Name;
			}
			return name;
		}

		internal static void ValidateBusinessProcess<T>(T businessProcess) {
			if (string.IsNullOrEmpty(GetBusinessProcessName(businessProcess.GetType()))) {
				throw new ArgumentException(
					$"Type {businessProcess.GetType().Name} must be decorated with BusinessProcessAttribute");
			}

			if (!(businessProcess is IBusinessProcess)) {
				throw new ArgumentException(
					$"Type {businessProcess.GetType().Name} must implement the IBusinessProcess interface");
			}
		}
	}
}
