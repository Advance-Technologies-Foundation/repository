namespace ATF.Repository
{
	using System;
	using System.Collections.Generic;

	public enum ModelState
	{
		New,
		Changed,
		Unchanged,
		Deleted
	}

	public interface IChangeTracker
	{
		IEnumerable<ITrackedModel<BaseModel>> GetTrackedModels();

		IEnumerable<ITrackedModel<T>> GetTrackedModels<T>() where T : BaseModel;
		ITrackedModel<T> GetTrackedModel<T>(T model) where T : BaseModel;
	}

	public interface ITrackedModel<out T> where T: BaseModel
	{
		T Model { get; }

		Type Type { get; }

		DateTime RegisteredTime { get; }

		ModelState GetStatus();

		Dictionary<string, object> GetChanges();
	}

	internal interface ITrackedModel
	{
		void ReloadInitValues();
	}
}
