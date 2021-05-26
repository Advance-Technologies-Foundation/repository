using System;
using System.Collections.Generic;
using System.Linq;

namespace ATF.Repository
{
	internal class AppDataContextChangeTracker: IChangeTracker
	{
		private readonly List<ITrackedModel<BaseModel>> _registry;

		internal AppDataContextChangeTracker() {
			_registry = new List<ITrackedModel<BaseModel>>();
		}

		internal void RegistryModel<T>(T model) where T: BaseModel {
			_registry.Add(new TrackedModel<T>(model));
		}

		internal void UnRegistryModel<T>(T model) where T: BaseModel {
			if (_registry.Any(x => x.Model == model)) {
				_registry.Remove(_registry.First(x => x.Model == model));
			}
		}

		internal ITrackedModel<BaseModel> GetTrackedModel(Type type, Guid id) {
			return _registry.FirstOrDefault(item => item.Type == type && item.Model.Id == id);
		}

		public IEnumerable<ITrackedModel<BaseModel>> GetTrackedModels() {
			return _registry;
		}

		public IEnumerable<ITrackedModel<T>> GetTrackedModels<T>() where T : BaseModel {
			var type = typeof(T);
			return _registry.Where(x=>x.Type == type).Select(x=>(ITrackedModel<T>)x);
		}

		public ITrackedModel<T> GetTrackedModel<T>(T model) where T : BaseModel {
			return (ITrackedModel<T>)_registry.FirstOrDefault(item => item.Type == typeof(T) && item.Model.Id == model.Id);
		}
	}
}
