using System.Collections.Generic;
using System.Linq;

namespace ATF.Repository
{
	internal class ChangeTracker: IChangeTracker
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

		private ITrackedModel<T> ConvertModelToTrackedModel<T>(T model) where T : BaseModel {
			return new TrackedModel<T> { Model = model };
		}
	}
}