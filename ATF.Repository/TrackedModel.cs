using System;
using System.Collections.Generic;
using System.Linq;
using Terrasoft.Common;

namespace ATF.Repository
{
	internal class TrackedModel<T>: ITrackedModel, ITrackedModel<T> where T : BaseModel
	{
		private Dictionary<string, object> _initValues;

		public DateTime RegisteredTime { get; }

		public Type Type { get; }

		public T Model { get; }

		internal TrackedModel(T model) {
			Model = model;
			RegisteredTime = DateTime.Now;
			Type = typeof(T);
			ReloadInitValues();
		}

		public void ReloadInitValues() {
			_initValues = Model?.GetModelPropertyValues();
		}

		public ModelState GetStatus() {
			if (Model.IsMarkAsDeleted) {
				return ModelState.Deleted;
			}

			if (Model.IsNew) {
				return ModelState.New;
			}

			var changes = GetChanges();
			return changes.Any() ? ModelState.Changed : ModelState.Unchanged;
		}

		public Dictionary<string, object> GetChanges() {
			var currentModelValues = Model?.GetModelPropertyValues() ?? new Dictionary<string, object>();
			var response = new Dictionary<string, object>();
			currentModelValues.ForEach(item => {
				if (_initValues.ContainsKey(item.Key) && !Equals(_initValues[item.Key], item.Value)) {
					response.Add(item.Key, item.Value);
				}
			});
			return response;
		}

	}
}
