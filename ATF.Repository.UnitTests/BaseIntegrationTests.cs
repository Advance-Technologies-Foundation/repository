namespace ATF.Repository.UnitTests
{
	using ATF.Repository.Providers;

	#region Class: BaseIntegrationTests

	public abstract class BaseIntegrationTests
	{
		#region Methods: Protected

		protected IDataProvider GetIntegrationDataProvider() {
			return new RemoteDataProvider("", "", "");
		}

		#endregion
		
	}

	#endregion
	
}