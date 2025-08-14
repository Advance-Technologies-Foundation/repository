using ATF.Repository.Providers;

namespace ATF.Repository
{
	public interface IAppProcessContext
	{
		IBusinessProcessResponse<T> RunProcess<T>(T process) where T : IBusinessProcess, new();
	}
}