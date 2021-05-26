namespace ATF.Repository
{
	using ATF.Repository.Providers;
	public class AppDataContextFactory
	{
		public static IAppDataContext GetAppDataContext(IDataProvider dataProvider)
		{
			return new AppDataContext(dataProvider);
		}
	}
}
