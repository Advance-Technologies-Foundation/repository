using System;
using System.Collections.Generic;
using System.Linq;
using ATF.Repository.Mapping;
using Terrasoft.Common;

namespace ATF.Repository.Builder
{
	internal class ModelBuilder
	{
		private readonly ILazyModelPropertyManager _lazyModelPropertyManager;
		private readonly ProxyClassBuilder _proxyClassBuilder;

		internal ModelBuilder(ILazyModelPropertyManager lazyModelPropertyManager) {
			_lazyModelPropertyManager = lazyModelPropertyManager;
			_proxyClassBuilder = new ProxyClassBuilder();
		}

		internal T Build<T>(Dictionary<string, object> values) where T : BaseModel, new() {
			var model = GetModelInstance<T>();
			ApplyValuesToModel(model, values);
			return model;
		}

		private T GetModelInstance<T>() where T : BaseModel, new() {
			var model = _proxyClassBuilder.Build<T>();
			model.Id = Guid.NewGuid();
			model.LazyModelPropertyManager = _lazyModelPropertyManager;
			return model;
		}

		private void ApplyValuesToModel<T>(T model, Dictionary<string, object> values)
			where T : BaseModel, new() {
			ApplyPropertyValuesToModel<T>(model, values);
			ApplyLazyLookupValuesToModel<T>(model, values);
		}

		private void ApplyLazyLookupValuesToModel<T>(T model, Dictionary<string, object> values)
			where T : BaseModel, new() {
			var properties = ModelMapper.GetProperties(model.GetType());
			var lazyLookupProperties = ModelMapper.GetLookups(model.GetType())
				.Where(lp => properties.All(x => x.EntityColumnName != lp.EntityColumnName));
			foreach (var property in lazyLookupProperties) {
				if (values.ContainsKey(property.EntityColumnName) && values[property.EntityColumnName] != null &&
				    values[property.EntityColumnName] is Guid guidValue) {
					model.SetLazyLookupKeyValue(property.PropertyName, guidValue);
				}
			}
		}

		private void ApplyPropertyValuesToModel<T>(T model, Dictionary<string, object> values)
			where T : BaseModel, new() {
			var properties = ModelMapper.GetProperties(model.GetType());
			foreach (var property in properties.Where(property => values.ContainsKey(property.EntityColumnName))) {
				if (property.EntityColumnName == "Id") {
					model.Id = (Guid) values[property.EntityColumnName];
				} else {
					model.SetPropertyValue(property.PropertyName, values[property.EntityColumnName]);
				}
			}
		}
	}
}
