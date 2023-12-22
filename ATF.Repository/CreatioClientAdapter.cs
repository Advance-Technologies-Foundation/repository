namespace ATF.Repository
{
	using Creatio.Client;

	public interface ICreatioClientAdapter
	{
		string ExecutePostRequest(string url, string requestData, int requestTimeout);
	}

	internal class CreatioClientAdapter: ICreatioClientAdapter
	{
		private CreatioClient _creatioClient;
		internal CreatioClientAdapter(string applicationUrl, string username, string password, bool isNetCore = false) {
			_creatioClient = new CreatioClient(applicationUrl, username, password, isNetCore);
		}

		internal CreatioClientAdapter(string applicationUrl, string authApp, string clientId, string clientSecret, bool isNetCore = false) {
			_creatioClient =
				CreatioClient.CreateOAuth20Client(applicationUrl, authApp, clientId, clientSecret, isNetCore);
		}

		public virtual string ExecutePostRequest(string url, string requestData, int requestTimeout) {
			return _creatioClient.ExecutePostRequest(url, requestData, requestTimeout);
		}
	}
}
