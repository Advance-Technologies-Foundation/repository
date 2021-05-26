using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ATF.Repository.Exceptions;
using ATF.Repository.Mapping;
using Terrasoft.Common;

namespace ATF.Repository
{
	internal class LazyModelPropertyLoader : ILazyModelPropertyLoader
	{
		private AppDataContext _appDataContext;

		internal LazyModelPropertyLoader(AppDataContext appDataContext) {
			_appDataContext = appDataContext;
		}

		public void LoadLazyProperty(BaseModel model, ModelItem propertyInfo) {
			if (propertyInfo.PropertyType == ModelItemType.Lookup) {
				LoadLazyLookupProperty(model, propertyInfo);
				return;
			}
			if (propertyInfo.PropertyType == ModelItemType.Detail) {
				LoadLazyDetailProperty(model, propertyInfo);
				return;
			}
			throw new NotImplementedException();
		}

		private void LoadLazyDetailProperty(BaseModel model, ModelItem propertyInfo) {
			var values = LoadDetailValues(model, propertyInfo);
			model.SetPropertyValue(propertyInfo.PropertyName, values);
		}

		private object LoadDetailValues(BaseModel model, ModelItem propertyInfo) {
			string detailLinkPropertyName = GetDetailLinkPropertyName(propertyInfo);
			var method = RepositoryReflectionUtilities.GetGenericMethod(GetType(), "LoadTypedDetailValues", propertyInfo.DataValueType);
			return method?.Invoke(this, new object[] {detailLinkPropertyName, model.Id});
		}

		private string GetDetailLinkPropertyName(ModelItem propertyInfo) {
			if (!typeof(BaseModel).IsAssignableFrom(propertyInfo.DataValueType)) {
				throw new UnexpectedDetailTypeException();
			}

			var detailProperties = ModelMapper.GetProperties(propertyInfo.DataValueType);
			if (detailProperties.Any(p => p.PropertyName == propertyInfo.DetailLinkPropertyName)) {
				return propertyInfo.DetailLinkPropertyName;
			}

			var sameProperties = detailProperties.Where(p =>
				p.EntityColumnName == propertyInfo.DetailLinkPropertyName && p.DataValueType == typeof(Guid)).ToList();
			if (sameProperties.Any()) {
				return sameProperties.First().PropertyName;
			}
			throw new NoneDetailLinkPropertyException();
		}

		private List<T> LoadTypedDetailValues<T>(string propertyName, Guid value) where T: BaseModel, new() {
			var expression = GenerateExpression<T>(propertyName, value);
			return _appDataContext.Models<T>().Where(expression).ToList();
		}

		private void LoadLazyLookupProperty(BaseModel model, ModelItem propertyInfo) {
			var idValue = GetIdValue(model, propertyInfo);
			if (idValue != Guid.Empty) {
				var lookupValue = LoadLookupValue(propertyInfo, idValue);
				model.SetPropertyValue(propertyInfo.PropertyName, lookupValue);
			} else {
				model.SetPropertyValue(propertyInfo.PropertyName, null);
			}
		}

		private BaseModel LoadLookupValue(ModelItem propertyInfo, Guid idValue) {
			var method = RepositoryReflectionUtilities.GetGenericMethod(GetType(), "LoadTypedLookupValue", propertyInfo.DataValueType);
			var referenceModel = (BaseModel)method?.Invoke(this, new object[] {idValue});
			return referenceModel;
		}

		private T LoadTypedLookupValue<T>(Guid value) where T: BaseModel, new() {
			var expression = GenerateExpression<T>("Id", value);
			var models = _appDataContext.Models<T>().Where(expression).ToList();
			return models.Any() ? models.First() : null;
		}

		private Expression<Func<T, bool>> GenerateExpression<T>(string propertyName, Guid value) {
			var parameterExpression = Expression.Parameter(typeof(T), "model");
			var leftExpression = Expression.Property(parameterExpression, propertyName);
			var rightExpression = Expression.Constant(value);
			var comparisonExpression = Expression.Equal(leftExpression, rightExpression);
			return Expression.Lambda<Func<T, bool>>(comparisonExpression, parameterExpression);
		}

		private Expression GenerateConstantExpression(Guid value, Type expressionDataValueType) {
			return Expression.Constant(value);
		}

		private Expression GenerateModelPropertyExpression(ParameterExpression parameterExpression, string propertyName) {
			return Expression.Property(parameterExpression, propertyName);
		}

		private Guid GetIdValue(BaseModel model, ModelItem propertyInfo) {
			var declaringType = propertyInfo.PropertyInfo.DeclaringType;
			var properties = ModelMapper.GetProperties(declaringType);
			if (properties.Any(x => x.DataValueType == typeof(Guid) && x.EntityColumnName == propertyInfo.EntityColumnName)) {
				return GetIdValueFromLocalProperty(model,
					properties.First(x =>
						x.DataValueType == typeof(Guid) && x.EntityColumnName == propertyInfo.EntityColumnName));
			}
			return GetIdValueFromInitialProperty(model, propertyInfo);
		}

		private Guid GetIdValueFromInitialProperty(BaseModel model, ModelItem propertyInfo) {
			return model.GetLazyLookupKeyValue(propertyInfo.PropertyName);
		}

		private static Guid GetIdValueFromLocalProperty(BaseModel model, ModelItem localModelItem) {
			var value = model.GetPropertyValue(localModelItem.PropertyName);
			return (Guid?) value ?? Guid.Empty;
		}
	}
}
