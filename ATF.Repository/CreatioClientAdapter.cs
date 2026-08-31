using System.Net;

namespace ATF.Repository
{
	using System;
	using Creatio.Client;

	public interface ICreatioClientAdapter
	{
		string ExecutePostRequest(string url, string requestData, int requestTimeout);
	}

	internal class CreatioClientAdapter: ICreatioClientAdapter, IDisposable
	{
		private readonly CreatioClient _creatioClient;
		internal CreatioClientAdapter(string applicationUrl, string username, string password, bool isNetCore = false) {
			_creatioClient = new CreatioClient(applicationUrl, username, password, isNetCore);
		}

		internal CreatioClientAdapter(string applicationUrl, string username, string password,
			bool useUntrustedSsl, bool isNetCore) {
			_creatioClient = new CreatioClient(applicationUrl, username, password, useUntrustedSsl,
				isNetCore);
		}
	
		internal CreatioClientAdapter(string applicationUrl, ICredentials credentials, bool isNetCore = false) {
			_creatioClient = new CreatioClient(applicationUrl, true, credentials, isNetCore);
		}

		internal CreatioClientAdapter(string applicationUrl, string authApp, string clientId, string clientSecret, bool isNetCore = false) {
			_creatioClient =
				CreatioClient.CreateOAuth20Client(applicationUrl, authApp, clientId, clientSecret, isNetCore);
		}

		internal CreatioClientAdapter(string applicationUrl, string bearerToken, bool isNetCore = false) {
			_creatioClient = new CreatioClient(applicationUrl, bearerToken, isNetCore);
		}

		internal CreatioClientAdapter(string applicationUrl, string bearerToken, bool useUntrustedSsl,
			bool isNetCore) {
			_creatioClient = new CreatioClient(applicationUrl, bearerToken, useUntrustedSsl, isNetCore);
		}

		public virtual string ExecutePostRequest(string url, string requestData, int requestTimeout) {
			return _creatioClient.ExecutePostRequest(url, requestData, requestTimeout);
		}

		public void Dispose() {
			_creatioClient.Dispose();
		}
	}
}
