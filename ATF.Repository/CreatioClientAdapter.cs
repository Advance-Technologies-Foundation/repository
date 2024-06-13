using System.Net;

namespace ATF.Repository
{
	using Creatio.Client;

	public interface ICreatioClientAdapter
	{
		string ExecutePostRequest(string url, string requestData, int requestTimeout, int retryCount = 3, int retryDelay = 1);
	}

	internal class CreatioClientAdapter: ICreatioClientAdapter
	{
		private ICreatioClient _creatioClient;
		
		internal CreatioClientAdapter(ICreatioClient client) {
			_creatioClient = client;
		}
		
		internal CreatioClientAdapter(string applicationUrl, string username, string password, bool isNetCore = false) {
			_creatioClient = new CreatioClient(applicationUrl, username, password, isNetCore);
		}
	
		internal CreatioClientAdapter(string applicationUrl, ICredentials credentials, bool isNetCore = false) {
			_creatioClient = new CreatioClient(applicationUrl, true, credentials, isNetCore);
		}

		internal CreatioClientAdapter(string applicationUrl, string authApp, string clientId, string clientSecret, bool isNetCore = false) {
			_creatioClient =
				CreatioClient.CreateOAuth20Client(applicationUrl, authApp, clientId, clientSecret, isNetCore);
		}

		public virtual string ExecutePostRequest(string url, string requestData, int requestTimeout, int retryCount = 3, int retryDelay = 1) {
			return _creatioClient.ExecutePostRequest(url, requestData, requestTimeout, retryCount, retryDelay);
		}
	}
}
