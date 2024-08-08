namespace ATF.Repository.UnitTests
{
	using System;
	using System.Linq;
	using ATF.Repository.Providers;
	using ATF.Repository.UnitTests.Utilities;
	using Newtonsoft.Json;
	using NSubstitute;
	using NUnit.Framework;
	using Terrasoft.Core.Entities;
	using Terrasoft.Nui.ServiceModel.DataContract;

	[TestFixture(Category = "UnitTests")]
	public class RemoteDataProviderTests
	{
		private RemoteDataProvider _remoteDataProvider;
		private ICreatioClientAdapter _creatioClientAdapter;

		private void MockExecutePostRequest(string request, string response) {
			_creatioClientAdapter
				.ExecutePostRequest(Arg.Any<string>(), Arg.Is<string>(x => x == request), Arg.Any<int>())
				.Returns(response);
		}

		[SetUp]
		public void SetUp() {
			_creatioClientAdapter = Substitute.For<ICreatioClientAdapter>();
			_remoteDataProvider = new RemoteDataProvider("", "", "") {CreatioClientAdapter = _creatioClientAdapter};
		}

		[Test]
		public void GetItems_WhenRequestSimpleSelect_ReturnExpectedValues() {
			var select = QueryBuilderUtilities.BuildSelectQuery("AccountType");
			QueryBuilderUtilities.AddColumn(select, "Id");
			QueryBuilderUtilities.AddColumn(select, "Name");
			MockExecutePostRequest(
				JsonConvert.SerializeObject(select),
				"{\"rowConfig\":{\"Id\":{\"dataValueType\":0},\"Name\":{\"dataValueType\":1}},\"rows\":[{\"Id\":\"f2c0ce97-53e6-df11-971b-001d60e938c6\",\"Name\":\"Партнер\"},{\"Id\":\"57412fad-53e6-df11-971b-001d60e938c6\",\"Name\":\"Наша компания\"},{\"Id\":\"2b6b75b6-d794-47bf-b5df-31dd95aa012d\",\"Name\":\"Клиент\"},{\"Id\":\"47100649-74e0-44e9-a9ae-36004b8d03be\",\"Name\":\"Не определен\"},{\"Id\":\"ea99196c-bbe1-4f2b-951e-49c57425efb5\",\"Name\":\"Конкурент\"},{\"Id\":\"1165192e-e578-40bb-b7f9-569e6a3a7745\",\"Name\":\"Потенциальный партнер\"},{\"Id\":\"1f01baf0-64f4-443d-9e16-71ab53ccc1e6\",\"Name\":\"СМИ\"},{\"Id\":\"2dd4ed36-d652-4b10-a7fd-8ed853361785\",\"Name\":\"Поставщик\"},{\"Id\":\"be4dc5a1-88c7-493f-8c40-b70fd769a745\",\"Name\":\"Инвестор\"}],\"notFoundColumns\":[],\"rowsAffected\":9,\"nextPrcElReady\":false,\"success\":true}"
			);

			var response = _remoteDataProvider.GetItems(select);
			Assert.IsTrue(response.Success);
			Assert.AreEqual(9, response.Items.Count);
			Assert.IsTrue(response.Items.All(x => (Guid) x["Id"] != Guid.Empty));
			Assert.IsTrue(response.Items.All(x => !string.IsNullOrEmpty(x["Name"].ToString())));
			Assert.IsTrue(response.Items.All(x => x.Count == 2));
		}

		[Test]
		public void GetItems_WhenRequestWithTripleFilter_ReturnExpectedValues2() {
			var selectQuery = QueryBuilderUtilities.BuildSelectQuery("Contact", 10, 5);
			QueryBuilderUtilities.AddColumn(selectQuery, "Id");
			QueryBuilderUtilities.AddColumn(selectQuery, "Name");
			QueryBuilderUtilities.AddColumn(selectQuery, "Email");
			QueryBuilderUtilities.AddColumn(selectQuery, "Account");
			QueryBuilderUtilities.AddColumn(selectQuery, "Type");
			QueryBuilderUtilities.AddColumn(selectQuery, "ContactSource");
			QueryBuilderUtilities.AddColumn(selectQuery, "Phone");
			selectQuery.Filters.Items.Add("f1",
				QueryBuilderUtilities.CreateComparisonFilter("Account.ExactNoOfEmployees", FilterComparisonType.Greater,
					DataValueType.Integer, 20));
			selectQuery.Filters.Items.Add("f2",
				QueryBuilderUtilities.CreateComparisonFilter("Phone", FilterComparisonType.IsNull, DataValueType.Text));
			selectQuery.Filters.Items.Add("f3",
				QueryBuilderUtilities.CreateComparisonFilter("Account.AccountCategory", FilterComparisonType.Equal,
					DataValueType.Guid, "67c9e487-53fe-412d-800d-ff98c26f55a0"));

			MockExecutePostRequest(JsonConvert.SerializeObject(selectQuery),
				"{\"rowConfig\":{\"Id\":{\"dataValueType\":0},\"Name\":{\"dataValueType\":1},\"Email\":{\"dataValueType\":1},\"Account\":{\"dataValueType\":10,\"isLookup\":true,\"referenceSchemaName\":\"Account\",\"primaryImageColumnName\":\"AccountLogo\"},\"Type\":{\"dataValueType\":10,\"isLookup\":true,\"referenceSchemaName\":\"ContactType\"},\"ContactSource\":{\"dataValueType\":10,\"isLookup\":true,\"referenceSchemaName\":\"ContactSource\"},\"Phone\":{\"dataValueType\":1},\"Photo\":{\"dataValueType\":16,\"isLookup\":true,\"referenceSchemaName\":\"SysImage\"}},\"rows\":[{\"Name\":\"Tom Roemer\",\"Email\":\"troemer@telus.net\",\"Account\":{\"value\":\"4c36ab2d-13ce-472e-8c19-0012d923045c\",\"displayValue\":\"British Columbia Institute of Technology\",\"primaryImageValue\":\"\"},\"Type\":\"\",\"ContactSource\":{\"value\":\"518131b3-aa8e-40b1-918c-28c3a0c54932\",\"displayValue\":\"Lead Extractor\",\"primaryImageValue\":\"\"},\"Phone\":\"\",\"Id\":\"58cc5766-2475-4029-8a91-2f4671fa4fd5\",\"Photo\":\"\"},{\"Name\":\"Thomas Tafoya\",\"Email\":\"tafoya6212@hotmail.com\",\"Account\":{\"value\":\"dff0dacb-3b29-4f13-b971-0736f9aae2bf\",\"displayValue\":\"Domino's Pizza\",\"primaryImageValue\":\"\"},\"Type\":\"\",\"ContactSource\":{\"value\":\"4e9993d1-010a-44c9-bf66-9fc6860835a9\",\"displayValue\":\"Lead Extractor\",\"primaryImageValue\":\"\"},\"Phone\":\"\",\"Id\":\"5787be75-1e86-465f-bea1-0c2ef895d079\",\"Photo\":\"\"},{\"Name\":\"Thomas Tafoya\",\"Email\":\"tafoya6212@hotmail.com\",\"Account\":{\"value\":\"dff0dacb-3b29-4f13-b971-0736f9aae2bf\",\"displayValue\":\"Domino's Pizza\",\"primaryImageValue\":\"\"},\"Type\":\"\",\"ContactSource\":{\"value\":\"4e9993d1-010a-44c9-bf66-9fc6860835a9\",\"displayValue\":\"Lead Extractor\",\"primaryImageValue\":\"\"},\"Phone\":\"\",\"Id\":\"c9502f64-2468-4d4a-a98d-d49784a7a5d3\",\"Photo\":\"\"},{\"Name\":\"Schrodel Alexander\",\"Email\":\"alexander.schrodel@rehau.com\",\"Account\":{\"value\":\"94aa6dd8-f814-4eca-bb5d-07e2e9866677\",\"displayValue\":\"REHAU\",\"primaryImageValue\":\"\"},\"Type\":{\"value\":\"00783ef6-f36b-1410-a883-16d83cab0980\",\"displayValue\":\"Клиент\",\"primaryImageValue\":\"\"},\"ContactSource\":\"\",\"Phone\":\"\",\"Id\":\"24998437-9fb3-447d-af18-011c76d92841\",\"Photo\":\"\"},{\"Name\":\"Heldens Paul\",\"Email\":\"paul.heldens@rehau.com\",\"Account\":{\"value\":\"94aa6dd8-f814-4eca-bb5d-07e2e9866677\",\"displayValue\":\"REHAU\",\"primaryImageValue\":\"\"},\"Type\":{\"value\":\"00783ef6-f36b-1410-a883-16d83cab0980\",\"displayValue\":\"Клиент\",\"primaryImageValue\":\"\"},\"ContactSource\":\"\",\"Phone\":\"\",\"Id\":\"49953bc3-85be-4b7b-ab03-54c4d8f7a511\",\"Photo\":\"\"},{\"Name\":\"Mundt Andreas\",\"Email\":\"andreas.mundt@rehau.com\",\"Account\":{\"value\":\"94aa6dd8-f814-4eca-bb5d-07e2e9866677\",\"displayValue\":\"REHAU\",\"primaryImageValue\":\"\"},\"Type\":{\"value\":\"00783ef6-f36b-1410-a883-16d83cab0980\",\"displayValue\":\"Клиент\",\"primaryImageValue\":\"\"},\"ContactSource\":\"\",\"Phone\":\"\",\"Id\":\"4170fff7-1c9a-4234-90db-ad521e797d94\",\"Photo\":\"\"},{\"Name\":\"Marquardt Klaus\",\"Email\":\"klaus.marquardt@rehau.com\",\"Account\":{\"value\":\"94aa6dd8-f814-4eca-bb5d-07e2e9866677\",\"displayValue\":\"REHAU\",\"primaryImageValue\":\"\"},\"Type\":{\"value\":\"00783ef6-f36b-1410-a883-16d83cab0980\",\"displayValue\":\"Клиент\",\"primaryImageValue\":\"\"},\"ContactSource\":\"\",\"Phone\":\"\",\"Id\":\"1f43c9c7-d587-4182-a972-afe84c52a5b4\",\"Photo\":\"\"},{\"Name\":\"Frederik Boesch\",\"Email\":\"frederik.boesch@rehau.com\",\"Account\":{\"value\":\"94aa6dd8-f814-4eca-bb5d-07e2e9866677\",\"displayValue\":\"REHAU\",\"primaryImageValue\":\"\"},\"Type\":\"\",\"ContactSource\":{\"value\":\"518131b3-aa8e-40b1-918c-28c3a0c54932\",\"displayValue\":\"Lead Extractor\",\"primaryImageValue\":\"\"},\"Phone\":\"\",\"Id\":\"35dfa216-8ff6-48ed-90f5-da6deda4d500\",\"Photo\":\"\"},{\"Name\":\"Rob Frey\",\"Email\":\"robfrey@dls.net\",\"Account\":{\"value\":\"193b8158-a4b3-4f24-b1d9-087aacb96d10\",\"displayValue\":\"PharMEDium Healthcare\",\"primaryImageValue\":\"\"},\"Type\":\"\",\"ContactSource\":{\"value\":\"518131b3-aa8e-40b1-918c-28c3a0c54932\",\"displayValue\":\"Lead Extractor\",\"primaryImageValue\":\"\"},\"Phone\":\"\",\"Id\":\"3f30d597-f4e5-427f-af87-5caf95aba1ef\",\"Photo\":\"\"},{\"Name\":\"Jim Mcmahon\",\"Email\":\"jmcmahon@pharmedium.com\",\"Account\":{\"value\":\"193b8158-a4b3-4f24-b1d9-087aacb96d10\",\"displayValue\":\"PharMEDium Healthcare\",\"primaryImageValue\":\"\"},\"Type\":\"\",\"ContactSource\":{\"value\":\"518131b3-aa8e-40b1-918c-28c3a0c54932\",\"displayValue\":\"Lead Extractor\",\"primaryImageValue\":\"\"},\"Phone\":\"\",\"Id\":\"b69ae191-fa93-4f69-abb8-dc5a3e87232b\",\"Photo\":\"\"}],\"notFoundColumns\":[],\"rowsAffected\":10,\"nextPrcElReady\":false,\"success\":true}"
			);
			var response = _remoteDataProvider.GetItems(selectQuery);

			Assert.IsTrue(response.Success);
			Assert.AreEqual(10, response.Items.Count);
			Assert.IsTrue(response.Items.All(x => (Guid) x["Id"] != Guid.Empty));
			Assert.IsTrue(response.Items.All(x => !string.IsNullOrEmpty(x["Name"].ToString())));
			Assert.IsTrue(response.Items.All(x => x.Count == 7));
		}

		[Test]
		public void GetItems_WhenRequestWithFilterFilter_ReturnExpectedValues2() {
			var selectQuery = QueryBuilderUtilities.BuildSelectQuery("Contact", 10, 5);
			QueryBuilderUtilities.AddColumn(selectQuery, "Id");
			QueryBuilderUtilities.AddColumn(selectQuery, "Name");
			selectQuery.Filters.Items.Add("f1",
				QueryBuilderUtilities.CreateComparisonFilter("Id", FilterComparisonType.Equal, DataValueType.Guid,
					"410006e1-ca4e-4502-a9ec-e54d922d2c00"));

			MockExecutePostRequest(JsonConvert.SerializeObject(selectQuery),
				"{\"rowConfig\":{\"Id\":{\"dataValueType\":0},\"Name\":{\"dataValueType\":1},\"Photo\":{\"dataValueType\":16,\"isLookup\":true,\"referenceSchemaName\":\"SysImage\"}},\"rows\":[{\"Id\":\"410006e1-ca4e-4502-a9ec-e54d922d2c00\",\"Name\":\"Supervisor\",\"Photo\":\"\"}],\"notFoundColumns\":[],\"rowsAffected\":1,\"nextPrcElReady\":false,\"success\":true}"
			);
			var response = _remoteDataProvider.GetItems(selectQuery);

			Assert.IsTrue(response.Success);
			Assert.AreEqual(1, response.Items.Count);
			Assert.AreEqual(new Guid("410006e1-ca4e-4502-a9ec-e54d922d2c00"), response.Items.First()["Id"]);
			Assert.AreEqual("Supervisor", response.Items.First()["Name"]);
			Assert.AreEqual(2, response.Items.First().Count);
		}

		[Test]
		public void GetItems_WhenRequestWithNotEqualFilterFilter_ReturnExpectedValues2() {
			var selectQuery = QueryBuilderUtilities.BuildSelectQuery("Contact", 10, 5);
			QueryBuilderUtilities.AddColumn(selectQuery, "Id");
			QueryBuilderUtilities.AddColumn(selectQuery, "Name");
			selectQuery.Filters.Items.Add("f1",
				QueryBuilderUtilities.CreateComparisonFilter("Id", FilterComparisonType.Equal, DataValueType.Guid,
					"410006e1-ca4e-4502-a9ec-e54d922d2c00"));
			selectQuery.Filters.Items.Add("f2",
				QueryBuilderUtilities.CreateComparisonFilter("Name", FilterComparisonType.NotEqual, DataValueType.Text,
					"WrongName"));

			MockExecutePostRequest(JsonConvert.SerializeObject(selectQuery),
				"{\"rowConfig\":{\"Id\":{\"dataValueType\":0},\"Name\":{\"dataValueType\":1},\"Photo\":{\"dataValueType\":16,\"isLookup\":true,\"referenceSchemaName\":\"SysImage\"}},\"rows\":[{\"Id\":\"410006e1-ca4e-4502-a9ec-e54d922d2c00\",\"Name\":\"Supervisor\",\"Photo\":\"\"}],\"notFoundColumns\":[],\"rowsAffected\":1,\"nextPrcElReady\":false,\"success\":true}"
			);
			var response = _remoteDataProvider.GetItems(selectQuery);

			Assert.IsTrue(response.Success);
			Assert.AreEqual(1, response.Items.Count);
			Assert.AreEqual(new Guid("410006e1-ca4e-4502-a9ec-e54d922d2c00"), response.Items.First()["Id"]);
			Assert.AreEqual("Supervisor", response.Items.First()["Name"]);
			Assert.AreEqual(2, response.Items.First().Count);
		}


	}
}
