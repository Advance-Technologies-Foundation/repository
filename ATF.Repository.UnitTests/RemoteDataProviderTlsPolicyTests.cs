namespace ATF.Repository.UnitTests
{
	using System.Reflection;
	using ATF.Repository.Providers;
	using Creatio.Client;
	using NUnit.Framework;

	[TestFixture]
	public class RemoteDataProviderTlsPolicyTests
	{
		[TestCase(false, false)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(true, true)]
		[Description("Explicit forms and bearer constructors forward the requested certificate policy to CreatioClient")]
		public void ExplicitTlsConstructors_ShouldForwardPolicy_WhenAuthenticationModeIsSelected(
			bool bearer, bool useUntrustedSsl)
		{
			// Arrange and Act
			RemoteDataProvider provider = bearer
				? new RemoteDataProvider("https://localhost", "token", useUntrustedSsl, true)
				: new RemoteDataProvider("https://localhost", "user", "password", useUntrustedSsl,
					true);

			// Assert
			CreatioClientAdapter adapter = (CreatioClientAdapter)provider.CreatioClientAdapter;
			FieldInfo clientField = typeof(CreatioClientAdapter).GetField("_creatioClient",
				BindingFlags.Instance | BindingFlags.NonPublic);
			CreatioClient client = (CreatioClient)clientField.GetValue(adapter);
			FieldInfo tlsField = typeof(CreatioClient).GetField("_useUntrustedSsl",
				BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.That(tlsField.GetValue(client), Is.EqualTo(useUntrustedSsl),
				"the explicit certificate policy must reach the underlying transport");
		}

		[Test]
		[Description("Disposing a RemoteDataProvider releases its owned CreatioClient transport")]
		public void Dispose_ShouldReleaseCreatioClient_WhenProviderOwnsTransport()
		{
			// Arrange
			RemoteDataProvider provider = new RemoteDataProvider("https://localhost", "token", false,
				true);
			CreatioClientAdapter adapter = (CreatioClientAdapter)provider.CreatioClientAdapter;
			FieldInfo clientField = typeof(CreatioClientAdapter).GetField("_creatioClient",
				BindingFlags.Instance | BindingFlags.NonPublic);
			CreatioClient client = (CreatioClient)clientField.GetValue(adapter);
			FieldInfo disposedField = typeof(CreatioClient).GetField("_disposed",
				BindingFlags.Instance | BindingFlags.NonPublic);

			// Act
			provider.Dispose();
			provider.Dispose();

			// Assert
			Assert.That(disposedField.GetValue(client), Is.True,
				"provider disposal must cascade to the owned pooled HTTP transport and remain idempotent");
		}
	}
}
