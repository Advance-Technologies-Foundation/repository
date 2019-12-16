namespace ATF.Repository.Builder
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Mapping;
	using Castle.DynamicProxy;
	using Terrasoft.Common;

	internal static class InterceptorRepository
	{
		private static readonly IDictionary<Type, IInterceptor> _interceptors = new Dictionary<Type, IInterceptor>();

		public static IInterceptor Get(Type type) {
			return _interceptors.ContainsKey(type)
				? _interceptors[type]
				: null;
		}
		public static void Add(Type type, IInterceptor interceptor) {
			if (!_interceptors.ContainsKey(type)) {
				_interceptors.Add(type, interceptor);
			}
		}
	}

	public class ProxyClassBuilder
	{
		private readonly Repository _repository;
		private readonly ProxyGenerator _generator;

		public ProxyClassBuilder(Repository repository) {
			_repository = repository;
			_generator = new ProxyGenerator();
		}

		private IInterceptor GetInterceptor<T>() where T : BaseModel {
			Type type = typeof(T);
			var interceptor = InterceptorRepository.Get(type);
			if (interceptor == null) {
				interceptor =  new InstanceProxyHelper<T>();
				InterceptorRepository.Add(type, interceptor);
			}
			return interceptor;
		}

		public T Build<T>() where T : BaseModel, new() {
			var item = (T)_generator.CreateClassProxy(typeof(T), GetInterceptor<T>());
			item.Repository = _repository;
			return item;
		}
	}

	internal class InstanceProxyHelper<T> : IInterceptor where T : BaseModel
	{
		private readonly Dictionary<MethodInfo, PropertyInfo> _properties;
		private readonly Dictionary<string, ModelItem> _modelItems;

		public InstanceProxyHelper() {
			var modelMapper = new ModelMapper();
			_properties = new Dictionary<MethodInfo, PropertyInfo>();
			_modelItems = new Dictionary<string, ModelItem>();

			modelMapper.GetModelItems(typeof(T)).Where(x => x.IsLazy).ForEach(x => {
				_modelItems.Add(x.PropertyInfo.Name, x);
				_properties.Add(x.PropertyInfo.GetMethod, x.PropertyInfo);
				_properties.Add(x.PropertyInfo.SetMethod, x.PropertyInfo);
			});
		}

		private T GetProxy(IInvocation invocation) {
			return (T)invocation.Proxy;
		}

		private void InternalSet(IInvocation invocation, PropertyInfo property) {
			var proxy = GetProxy(invocation);
			proxy.LazyValues[property.Name] = invocation.Arguments[0];
		}

		private void FillProperty(IInvocation invocation, PropertyInfo property) {
			var proxy = GetProxy(invocation);
			var modelItem = _modelItems[property.Name];
			if (modelItem.PropertyType == ModelItemType.Reference) {
				proxy.Repository.FillReferenceValue((T)invocation.InvocationTarget, modelItem);
			} else if (modelItem.PropertyType == ModelItemType.Detail) {
				proxy.Repository.FillDetailValue((T)invocation.InvocationTarget, modelItem);
			} else if (modelItem.PropertyType == ModelItemType.Lookup) {
				proxy.Repository.FillLookupValue((T)invocation.InvocationTarget, modelItem);
			}
		}

		private void InternalGet(IInvocation invocation, PropertyInfo property) {
			var proxy = GetProxy(invocation);
			if (!proxy.LazyValues.ContainsKey(property.Name)) {
				FillProperty(invocation, property);
			}
			invocation.ReturnValue = proxy.LazyValues.ContainsKey(property.Name)
				? proxy.LazyValues[property.Name]
				: null;
		}

		public void Intercept(IInvocation invocation) {
			if (_properties.ContainsKey(invocation.Method)) {
				var property = _properties[invocation.Method];
				if (invocation.Method == property.SetMethod) {
					InternalSet(invocation, property);
				} else {
					InternalGet(invocation, property);
				}
			} else {
				invocation.Proceed();
			}
		}
	}

}