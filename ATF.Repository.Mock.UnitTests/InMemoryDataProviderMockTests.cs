namespace ATF.Repository.Mock.UnitTests
{
	using System;
	using System.Linq;
	using ATF.Repository.Mock.UnitTests.Models;
	using NUnit.Framework;

	[TestFixture]
	public class InMemoryDataProviderMockTests
	{
		private InMemoryDataProviderMock _inMemoryDataProviderMock;
		private IAppDataContext _appDataContext;
		private Guid _sysSettingsId;

		[SetUp]
		public void SetUp() {
			_inMemoryDataProviderMock = new InMemoryDataProviderMock();
			_inMemoryDataProviderMock.DataStore.RegisterModelSchema<SysSettings>();
			_inMemoryDataProviderMock.DataStore.RegisterModelSchema<SysSettingsValue>();
			_inMemoryDataProviderMock.DataStore.RegisterModelSchema<SysAdminUnit>();
			_appDataContext = AppDataContextFactory.GetAppDataContext(_inMemoryDataProviderMock);

			_sysSettingsId = Guid.NewGuid();
			_inMemoryDataProviderMock.DataStore.AddModel<SysSettings>(_sysSettingsId, model => {
				model.Name = "Use Freedom UI interface";
				model.Code = "UseNewShell";
				model.Description =
					"If the setting is enabled, then after logging in users will see Freedom interface. If the setting is disabled, then users will see classic Creatio interface";
				model.ValueTypeName = "Boolean";
				model.IsCacheable = true;
				model.IsPersonal = true;
			});
			_inMemoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.SysSettingsId = _sysSettingsId;
				model.BooleanValue = true;
				model.TextValue = "TextValue";
				model.IntegerValue = 11;
				model.FloatValue = 12.18m;
				model.DateTimeValue = DateTime.Now;
				model.GuidValue = Guid.NewGuid();
			});
		}

		[Test]
		public void Get_WhenFilterByString_ShouldReturnExpectedValues() {
			//Expression<Func<DataRow, bool>> action = row => row.Field<string>("Code") == "UseNewShell";
			//Expression<Func<DataRow, bool>> action2 = row => row.Field<string>("Code") == "UseNewShell";
			//sysSettingsTable.AsEnumerable().FirstOrDefault(x => x.Field<string>("Code") == "UseNewShell");
			//Expression<Func<DataRow, bool>> action = row => row.GetParentRow("rel") != null && row.GetParentRow("rel").Field<string>("Code") == "UseNewShell";
			var feature = _appDataContext.Models<SysSettings>().FirstOrDefault(x => x.Code == "UseNewShell");
			Assert.AreEqual("UseNewShell", feature?.Code ?? "");

			var featureNull = _appDataContext.Models<SysSettings>().FirstOrDefault(x => x.Code == "UseNewShell_Other");
			Assert.IsNull(featureNull);

			var featureNull2 = _appDataContext.Models<SysSettings>().FirstOrDefault(x => x.Code != "UseNewShell");
			Assert.IsNull(featureNull2);
		}

		[Test]
		public void Get_WhenFilterByBoolean_ShouldReturnExpectedValues() {
			var feature = _appDataContext.Models<SysSettings>().FirstOrDefault(x => x.IsCacheable);
			Assert.AreEqual("UseNewShell", feature?.Code ?? "");

			var featureNull = _appDataContext.Models<SysSettings>().FirstOrDefault(x => !x.IsCacheable);
			Assert.IsNull(featureNull);
		}

		[Test]
		public void Get_WhenFilterByDecimal_ByEqual_ShouldReturnExpectedValues() {
			var feature = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.FloatValue == 12.18m);
			Assert.AreEqual(_sysSettingsId, feature?.SysSettingsId ?? Guid.Empty);

			var featureNull = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.FloatValue == -12.18m);
			Assert.IsNull(featureNull);
		}

		[Test]
		public void Get_WhenFilterByDecimal_ByGreater_ShouldReturnExpectedValues() {
			var featureGreater = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.FloatValue > 11.18m);
			Assert.AreEqual(_sysSettingsId, featureGreater?.SysSettingsId ?? Guid.Empty);

			var featureGreaterNull = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.FloatValue > 15);
			Assert.IsNull(featureGreaterNull);
		}

		[Test]
		public void Get_WhenFilterByDecimal_ByGreaterOrEqual_ShouldReturnExpectedValues() {
			var featureGreaterOrEqual =
				_appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.FloatValue >= 11.18m);
			Assert.AreEqual(_sysSettingsId, featureGreaterOrEqual?.SysSettingsId ?? Guid.Empty);

			var featureGreaterOrEqualNull =
				_appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.FloatValue >= 15);
			Assert.IsNull(featureGreaterOrEqualNull);
		}

		[Test]
		public void Get_WhenFilterByDecimal_ByLess_ShouldReturnExpectedValues() {
			var featureLess = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.FloatValue < 13.18m);
			Assert.AreEqual(_sysSettingsId, featureLess?.SysSettingsId ?? Guid.Empty);

			var featureLessNull = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.FloatValue < 11);
			Assert.IsNull(featureLessNull);
		}

		[Test]
		public void Get_WhenFilterByDecimal_ByLessOrEqual_ShouldReturnExpectedValues() {
			var featureLessOrEqual =
				_appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.FloatValue <= 13.18m);
			Assert.AreEqual(_sysSettingsId, featureLessOrEqual?.SysSettingsId ?? Guid.Empty);

			var featureLessOrEqualNull =
				_appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.FloatValue <= 11);
			Assert.IsNull(featureLessOrEqualNull);
		}

		[Test]
		public void Get_WhenFilterByDecimal_ByNotEqual_ShouldReturnExpectedValues() {
			var featureNotEqual =
				_appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.FloatValue != 13.18m);
			Assert.AreEqual(_sysSettingsId, featureNotEqual?.SysSettingsId ?? Guid.Empty);

			var featureNotEqualNull =
				_appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.FloatValue != 12.18m);
			Assert.IsNull(featureNotEqualNull);
		}

		[Test]
		public void Get_WhenFilterByInt_ByEqual_ShouldReturnExpectedValues() {
			var feature = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.IntegerValue == 11);
			Assert.AreEqual(_sysSettingsId, feature?.SysSettingsId ?? Guid.Empty);

			var featureNull = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.IntegerValue == -11);
			Assert.IsNull(featureNull);
		}

		[Test]
		public void Get_WhenFilterByInt_ByGreater_ShouldReturnExpectedValues() {
			var featureGreater = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.IntegerValue > 10);
			Assert.AreEqual(_sysSettingsId, featureGreater?.SysSettingsId ?? Guid.Empty);

			var featureGreaterNull =
				_appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.IntegerValue > 12);
			Assert.IsNull(featureGreaterNull);
		}

		[Test]
		public void Get_WhenFilterByInt_ByGreaterOrEqual_ShouldReturnExpectedValues() {
			var featureGreaterOrEqual =
				_appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.IntegerValue >= 10);
			Assert.AreEqual(_sysSettingsId, featureGreaterOrEqual?.SysSettingsId ?? Guid.Empty);

			var featureGreaterOrEqualNull =
				_appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.IntegerValue >= 12);
			Assert.IsNull(featureGreaterOrEqualNull);
		}

		[Test]
		public void Get_WhenFilterByInt_ByLess_ShouldReturnExpectedValues() {
			var featureLess = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.IntegerValue < 12);
			Assert.AreEqual(_sysSettingsId, featureLess?.SysSettingsId ?? Guid.Empty);

			var featureLessNull = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.IntegerValue < 10);
			Assert.IsNull(featureLessNull);
		}

		[Test]
		public void Get_WhenFilterByInt_ByLessOrEqual_ShouldReturnExpectedValues() {
			var featureLessOrEqual =
				_appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.IntegerValue <= 12);
			Assert.AreEqual(_sysSettingsId, featureLessOrEqual?.SysSettingsId ?? Guid.Empty);

			var featureLessOrEqualNull =
				_appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.IntegerValue <= 10);
			Assert.IsNull(featureLessOrEqualNull);
		}

		[Test]
		public void Get_WhenFilterByInt_ByNotEqual_ShouldReturnExpectedValues() {
			var featureNotEqual = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.IntegerValue != 10);
			Assert.AreEqual(_sysSettingsId, featureNotEqual?.SysSettingsId ?? Guid.Empty);

			var featureNotEqualNull =
				_appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.IntegerValue != 11);
			Assert.IsNull(featureNotEqualNull);
		}

		[Test]
		public void Get_WhenFilterWithAnd_ShouldReturnExpectedValues() {
			_inMemoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.SysSettingsId = _sysSettingsId;
				model.BooleanValue = true;
				model.TextValue = "TextValue";
				model.IntegerValue = 21;
				model.FloatValue = 12.18m;
				model.DateTimeValue = DateTime.Now;
				model.GuidValue = Guid.NewGuid();
			});

			var featureNotEqual = _appDataContext.Models<SysSettingsValue>()
				.FirstOrDefault(x => x.IntegerValue > 20 && x.SysSettingsId == _sysSettingsId);
			Assert.AreEqual(_sysSettingsId, featureNotEqual?.SysSettingsId ?? Guid.Empty);
			Assert.AreEqual(21, featureNotEqual?.IntegerValue ?? 0);
		}

		[Test]
		public void Get_WhenFilterWithLookup_ShouldReturnExpectedValues() {
			var featureNotEqual = _appDataContext.Models<SysSettingsValue>()
				.FirstOrDefault(x => x.SysSettings.Code == "UseNewShell");
			Assert.AreEqual(_sysSettingsId, featureNotEqual?.SysSettingsId ?? Guid.Empty);
		}

		[Test]
		public void Get_WhenFilterWithLookupAndTableHasNullReference_ShouldReturnExpectedValues() {
			_inMemoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.BooleanValue = true;
				model.TextValue = "TextValue";
				model.IntegerValue = 21;
				model.FloatValue = 12.18m;
				model.DateTimeValue = DateTime.Now;
				model.GuidValue = Guid.NewGuid();
			});
			var list = _appDataContext.Models<SysSettingsValue>().Where(x => !x.SysSettings.IsSSPAvailable).ToList();
			Assert.AreEqual(1, list.Count);
		}
	}
}
