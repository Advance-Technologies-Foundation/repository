namespace ATF.Repository.Builder
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using ATF.Repository.Mapping;
	using Castle.DynamicProxy;

	public class ProxyClassBuilder
	{
		private IDictionary<Type, IInterceptor> _interceptors;

		private Repository _repository;
		private ProxyGenerator _generator;

		public ProxyClassBuilder(Repository repository) {
			_interceptors = new Dictionary<Type, IInterceptor>();
			_repository = repository;
			_generator = new ProxyGenerator();
		}

		private IInterceptor GetInterceptor<T>() where T : BaseModel {
			Type type = typeof(T);
			if (!_interceptors.ContainsKey(type)) {
				_interceptors[type] = new InstanceProxyHelper<T>();
			}
			return _interceptors[type];
		}

		public T Build<T>() where T : BaseModel, new() {
			var item = (T)_generator.CreateClassProxy(typeof(T), GetInterceptor<T>());
			item.Repository = _repository;
			return item;
		}
	}

	internal class InstanceProxyHelper<T> : IInterceptor where T : BaseModel
	{
		private ModelMapper _modelMapper;
		private Dictionary<string, LazyMapInfo> _maps;
		private Dictionary<MethodInfo, PropertyInfo> _properties;

		public InstanceProxyHelper() {
			_modelMapper = new ModelMapper();
			_maps = new Dictionary<string, LazyMapInfo>();
			_properties = new Dictionary<MethodInfo, PropertyInfo>();


			var references = _modelMapper.GetReferences(typeof(T)).Where(x => x.IsLazyLoad);
			var details = _modelMapper.GetDetails(typeof(T)).Where(x => x.IsLazyLoad);
			var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetMethod.IsVirtual);
			foreach (var property in properties) {
				var reference = references.Where(x => x.Name == property.Name).FirstOrDefault();
				var detail = details.Where(x => x.Name == property.Name).FirstOrDefault();
				if (reference != null || detail != null) {
					if (reference != null) {
						_maps.Add(property.Name, reference);
					}
					if (detail != null) {
						_maps.Add(property.Name, detail);
					}
					_properties.Add(property.GetMethod, property);
					_properties.Add(property.SetMethod, property);
				}
			}
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
			var map = _maps[property.Name];
			var reference = map as ModelReference;
			if (reference != null) {
				proxy.Repository.FillReferenceValue<T>((T)invocation.InvocationTarget, map as ModelReference);
				return;
			}
			var detail = map as ModelDetail;
			if (detail != null) {
				proxy.Repository.FillDetailValue<T>((T)invocation.InvocationTarget, map as ModelDetail);
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