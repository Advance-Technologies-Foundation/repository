using System.Linq;

namespace ATF.Repository
{
	public interface IAppDataContext
	{
		IChangeTracker ChangeTracker { get; }
		T CreateModel<T>() where T : BaseModel, new();
		IQueryable<T> Models<T>() where T : BaseModel, new();

		void DeleteModel<T>(T model) where T : BaseModel;

		ISaveResult Save();

	}
}
