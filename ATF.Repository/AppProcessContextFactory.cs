namespace ATF.Repository
{
	using ATF.Repository.Providers;

	#region Class: AppProcessContextFactory

	public class AppProcessContextFactory
	{
		#region Methods: Public

		public static IAppProcessContext GetAppProcessContext(IDataProvider dataProvider)
		{
			return new AppProcessContext(dataProvider);
		}

		#endregion

	}

	#endregion

}