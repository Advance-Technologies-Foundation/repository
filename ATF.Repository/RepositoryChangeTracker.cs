namespace ATF.Repository
{
	using System.Collections.Generic;
	using System.Linq;

	internal class RepositoryChangeTracker: IChangeTracker
	{
		internal Repository Repository { get; set; }
		public IEnumerable<ITrackedModel<BaseModel>> GetTrackedModels() {
			return Repository.Items.Select(item => ConvertModelToTrackedModel(item.Value));
		}

		public IEnumerable<ITrackedModel<T>> GetTrackedModels<T>() where T : BaseModel {
			return Repository.Items
				.Where(item => item.Value is T)
				.Select(item => ConvertModelToTrackedModel((T)item.Value));
		}

		public ITrackedModel<T> GetTrackedModel<T>(T model) where T : BaseModel {
			return Repository.Items.ContainsKey(model.Id)
				? new TrackedModel<T>(model)
				: null;
		}

		private ITrackedModel<T> ConvertModelToTrackedModel<T>(T model) where T : BaseModel {
			return new TrackedModel<T>(model);
		}
	}
}
