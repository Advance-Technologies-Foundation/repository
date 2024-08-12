namespace ATF.Repository.Mock.UnitTests
{
	using System.IO;
	using System.Reflection;
	using System.Linq;
	using ATF.Repository.Mock.UnitTests.Models;
	using NUnit.Framework;

	[TestFixture]
	public class FileToMemoryDataProviderMockTests
	{
		private MemoryDataProviderMock _memoryDataProviderMock;

		[SetUp]
		public void SetUp() {
			_memoryDataProviderMock = new MemoryDataProviderMock();
			_memoryDataProviderMock.DataStore.RegisterModelSchema<SysSettings>();
		}

		[Test]
		public void LoadDataFromFileStore_ShouldLoadExpectedDataToDataStore() {
			var appDataContext = AppDataContextFactory.GetAppDataContext(_memoryDataProviderMock);

			var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var resourcePath = Path.Combine(rootPath, "Resources");
			_memoryDataProviderMock.DataStore.LoadDataFromFileStore(resourcePath);

			// Assert
			var model = appDataContext.Models<SysSettings>()
				.FirstOrDefault(x => x.Code == "DefLookupEditPageSchemaUId");
			Assert.IsNotNull(model);
			Assert.IsTrue(model.SysSettingsValues.Any());
		}
	}
}