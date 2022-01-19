namespace ATF.Repository
{
	using System;
	using ATF.Repository.Providers;
	using System.Linq;

	public interface IAppDataContext
	{
		IChangeTracker ChangeTracker { get; }

		T CreateModel<T>() where T : BaseModel, new();

		IQueryable<T> Models<T>() where T : BaseModel, new();

		T GetModel<T>(Guid id) where T : BaseModel, new();

		void DeleteModel<T>(T model) where T : BaseModel;

		ISaveResult Save();

		ISysSettingResponse<T> GetSysSettingValue<T>(string sysSettingCode);

		IFeatureResponse GetFeatureEnabled(string featureCode);
	}
}
