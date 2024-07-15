namespace ATF.Repository.Mock.UnitTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.Mock.UnitTests.Models;
	using NUnit.Framework;

	[TestFixture]
	public class MemoryDataProviderMockTests
	{
		private MemoryDataProviderMock _memoryDataProviderMock;
		private IAppDataContext _appDataContext;
		private IAppDataContext _secondAppDataContext;
		private Guid _sysSettingsId;
		private Guid _sysAdminUnit;

		private DateTime TrimDateTime(DateTime dateTime) {
			return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
		}

		[SetUp]
		public void SetUp() {
			_memoryDataProviderMock = new MemoryDataProviderMock();
			_memoryDataProviderMock.DataStore.RegisterModelSchema<SysSettings>();
			_memoryDataProviderMock.DataStore.RegisterModelSchema<SysSettingsValue>();
			_memoryDataProviderMock.DataStore.RegisterModelSchema<SysAdminUnit>();
			_appDataContext = AppDataContextFactory.GetAppDataContext(_memoryDataProviderMock);
			_secondAppDataContext = AppDataContextFactory.GetAppDataContext(_memoryDataProviderMock);

			_sysSettingsId = Guid.NewGuid();
			_sysAdminUnit = Guid.NewGuid();
			_memoryDataProviderMock.DataStore.AddModel<SysAdminUnit>(_sysAdminUnit, model => {
				model.Name = "All employers";
			});
			_memoryDataProviderMock.DataStore.AddModel<SysSettings>(_sysSettingsId, model => {
				model.Name = "Use Freedom UI interface";
				model.Code = "UseNewShell";
				model.Description =
					"If the setting is enabled, then after logging in users will see Freedom interface. If the setting is disabled, then users will see classic Creatio interface";
				model.ValueTypeName = "Boolean";
				model.IsCacheable = true;
				model.IsPersonal = true;
			});

			_memoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.SysSettingsId = _sysSettingsId;
				model.SysAdminUnitId = _sysAdminUnit;
				model.BooleanValue = true;
				model.TextValue = "TextValue";
				model.IntegerValue = 11;
				model.FloatValue = 12.18m;
				model.DateTimeValue = DateTime.Now;
				model.GuidValue = Guid.NewGuid();
			});
		}

		private Guid SetUpSortData(DateTime dateTime) {
			var sysSettings = _memoryDataProviderMock.DataStore.AddModel<SysSettings>(model => {
				model.Name = "Use Freedom UI interface";
				model.Code = "CoreInter";
				model.Description = "";
				model.ValueTypeName = "Boolean";
				model.IsCacheable = true;
				model.IsPersonal = true;
			});

			_memoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.BooleanValue = true;
				model.TextValue = "TextValue additional";
				model.IntegerValue = 24;
				model.FloatValue = 12.18m;
				model.DateTimeValue = dateTime;
				model.GuidValue = Guid.NewGuid();
				model.SysSettingsId = sysSettings.Id;
			});
			_memoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.BooleanValue = true;
				model.TextValue = "TextValue additional";
				model.IntegerValue = 22;
				model.FloatValue = 12.18m;
				model.DateTimeValue = dateTime.AddDays(1);
				model.GuidValue = Guid.NewGuid();
				model.SysSettingsId = sysSettings.Id;
			});
			_memoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.BooleanValue = true;
				model.TextValue = "TextValue additional";
				model.IntegerValue = 23;
				model.FloatValue = 11.18m;
				model.DateTimeValue = dateTime.AddDays(2);
				model.GuidValue = Guid.NewGuid();
				model.SysSettingsId = sysSettings.Id;
			});

			return sysSettings.Id;
		}

		[Test]
		public void Get_WhenFilterByString_ShouldReturnExpectedValues() {
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

			var feature2 = _appDataContext.Models<SysSettings>().FirstOrDefault(x => x.IsCacheable == true);
			Assert.AreEqual("UseNewShell", feature2?.Code ?? "");

			var featureNull2 = _appDataContext.Models<SysSettings>().FirstOrDefault(x => x.IsCacheable == false);
			Assert.IsNull(featureNull2);
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
		public void Get_WhenFilterByDateTime_ByEqual_ShouldReturnExpectedValues() {
			var model = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.DateTimeValue == DateTime.Now);
			Assert.AreEqual(_sysSettingsId, model?.SysSettingsId ?? Guid.Empty);
		}

		[Test]
		public void Get_WhenFilterByDateTime_ByGreater_ShouldReturnExpectedValues() {
			var model = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.DateTimeValue > DateTime.Now.AddDays(-1));
			Assert.AreEqual(_sysSettingsId, model?.SysSettingsId ?? Guid.Empty);
		}

		[Test]
		public void Get_WhenFilterByDateTime_ByGreaterOrEqual_ShouldReturnExpectedValues() {
			var model1 = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.DateTimeValue >= DateTime.Now.AddDays(-1));
			Assert.AreEqual(_sysSettingsId, model1?.SysSettingsId ?? Guid.Empty);

			var model2 = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.DateTimeValue >= DateTime.Now);
			Assert.AreEqual(_sysSettingsId, model2?.SysSettingsId ?? Guid.Empty);
		}

		[Test]
		public void Get_WhenFilterByDateTime_ByLess_ShouldReturnExpectedValues() {
			var model = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.DateTimeValue < DateTime.Now.AddDays(1));
			Assert.AreEqual(_sysSettingsId, model?.SysSettingsId ?? Guid.Empty);
		}

		[Test]
		public void Get_WhenFilterByDateTime_ByLessOrEqual_ShouldReturnExpectedValues() {
			var model1 = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.DateTimeValue <= DateTime.Now.AddDays(1));
			Assert.AreEqual(_sysSettingsId, model1?.SysSettingsId ?? Guid.Empty);

			var model2 = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.DateTimeValue <= DateTime.Now);
			Assert.AreEqual(_sysSettingsId, model2?.SysSettingsId ?? Guid.Empty);
		}

		[Test]
		public void Get_WhenFilterByDateTime_ByNotEqual_ShouldReturnExpectedValues() {
			var featureNotEqual = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.DateTimeValue != DateTime.Now.AddDays(-1));
			Assert.AreEqual(_sysSettingsId, featureNotEqual?.SysSettingsId ?? Guid.Empty);
		}

		[Test]
		public void Get_WhenFilterWithAnd_ShouldReturnExpectedValues() {
			_memoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
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
			_memoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
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

		[Test]
		public void Get_WhenUseSeveralFiltersWithLookupAndTableHasNullReference_ShouldReturnExpectedValues() {
			_memoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.BooleanValue = true;
				model.TextValue = "TextValue";
				model.IntegerValue = 21;
				model.FloatValue = 12.18m;
				model.DateTimeValue = DateTime.Now;
				model.GuidValue = Guid.NewGuid();
			});
			var list = _appDataContext.Models<SysSettingsValue>().Where(x => !x.SysSettings.IsSSPAvailable && x.SysAdminUnit.Name != "Hello").ToList();
			Assert.AreEqual(1, list.Count);
		}

		[Test]
		public void Get_WhenUseDetailAnyFilters_ShouldReturnExpectedValues() {
			_memoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.BooleanValue = true;
				model.TextValue = "TextValue";
				model.IntegerValue = 21;
				model.FloatValue = 12.18m;
				model.DateTimeValue = DateTime.Now;
				model.GuidValue = Guid.NewGuid();
				model.SysSettingsId = _sysSettingsId;
			});
			var list = _appDataContext.Models<SysSettings>().Where(x => x.SysSettingsValues.Any(y=>y.IntegerValue == 11)).ToList();
			Assert.AreEqual(1, list.Count);

			var listWithoutSubFilter = _appDataContext.Models<SysSettings>().Where(x => x.SysSettingsValues.Any()).ToList();
			Assert.AreEqual(1, listWithoutSubFilter.Count);

			var listWithRightSubFilter = _appDataContext.Models<SysSettings>().Where(x => x.SysSettingsValues.Any() == true).ToList();
			Assert.AreEqual(1, listWithRightSubFilter.Count);

			var listNotExists = _appDataContext.Models<SysSettings>().Where(x => x.SysSettingsValues.Any(y=>y.IntegerValue == 31)).ToList();
			Assert.AreEqual(0, listNotExists.Count);
		}

		[Test]
		public void Get_WhenUseDetailSumFilters_ShouldReturnExpectedValues() {
			_memoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.BooleanValue = true;
				model.TextValue = "TextValue";
				model.IntegerValue = 21;
				model.FloatValue = 12.18m;
				model.DateTimeValue = DateTime.Now;
				model.GuidValue = Guid.NewGuid();
			});
			var list = _appDataContext.Models<SysSettings>()
				.Where(x => x.SysSettingsValues.Where(y => y.BooleanValue).Sum(y => y.IntegerValue) == 11).ToList();
			Assert.AreEqual(1, list.Count);
		}

		[Test]
		public void Get_WhenUseDetailMinFilters_ShouldReturnExpectedValues() {
			_memoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.BooleanValue = true;
				model.TextValue = "TextValue additional";
				model.IntegerValue = 21;
				model.FloatValue = 12.18m;
				model.DateTimeValue = DateTime.Now.AddYears(-1);
				model.GuidValue = Guid.NewGuid();
				model.SysSettingsId = _sysSettingsId;
			});
			var list = _appDataContext.Models<SysSettings>().Where(x => x.SysSettingsValues.Where(y=>y.BooleanValue).Min(y=>y.DateTimeValue) < DateTime.Now.AddYears(-1).AddDays(1)).ToList();
			Assert.AreEqual(1, list.Count);
		}

		[Test]
		public void Get_WhenUseDetailMaxFilters_ShouldReturnExpectedValues() {
			_memoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.BooleanValue = true;
				model.TextValue = "TextValue additional";
				model.IntegerValue = 21;
				model.FloatValue = 12.18m;
				model.DateTimeValue = DateTime.Now.AddDays(2);
				model.GuidValue = Guid.NewGuid();
				model.SysSettingsId = _sysSettingsId;
			});
			var list = _appDataContext.Models<SysSettings>().Where(x => x.SysSettingsValues.Where(y=>y.BooleanValue).Max(y=>y.DateTimeValue) > DateTime.Now.AddDays(1)).ToList();
			Assert.AreEqual(1, list.Count);

			var listZero = _appDataContext.Models<SysSettings>().Where(x => x.SysSettingsValues.Where(y=>y.BooleanValue).Max(y=>y.DateTimeValue) < DateTime.Now.AddDays(1)).ToList();
			Assert.AreEqual(0, listZero.Count);
		}

		[Test]
		public void Get_WhenUseDetailAverageFilters_ShouldReturnExpectedValues() {
			_memoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.BooleanValue = true;
				model.TextValue = "TextValue additional";
				model.IntegerValue = 21;
				model.FloatValue = 12.18m;
				model.DateTimeValue = DateTime.Now.AddDays(2);
				model.GuidValue = Guid.NewGuid();
				model.SysSettingsId = _sysSettingsId;
			});
			var list = _appDataContext.Models<SysSettings>().Where(x => x.SysSettingsValues.Where(y=>y.BooleanValue).Average(y=>y.FloatValue) > 10).ToList();
			Assert.AreEqual(1, list.Count);

			var listNull = _appDataContext.Models<SysSettings>().Where(x => x.SysSettingsValues.Where(y=>y.BooleanValue).Average(y=>y.FloatValue) < 10).ToList();
			Assert.AreEqual(0, listNull.Count);
		}

		[Test]
		public void Get_WhenUseDetailCountFilters_ShouldReturnExpectedValues() {
			_memoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.BooleanValue = true;
				model.TextValue = "TextValue additional";
				model.IntegerValue = 21;
				model.FloatValue = 12.18m;
				model.DateTimeValue = DateTime.Now.AddDays(2);
				model.GuidValue = Guid.NewGuid();
				model.SysSettingsId = _sysSettingsId;
			});
			var list = _appDataContext.Models<SysSettings>().Where(x => x.SysSettingsValues.Where(y=>y.BooleanValue).Count() == 2).ToList();
			Assert.AreEqual(1, list.Count);

			var list2 = _appDataContext.Models<SysSettings>().Where(x => x.SysSettingsValues.Count(y=>y.BooleanValue) == 2).ToList();
			Assert.AreEqual(1, list2.Count);

		}

		[Test]
		public void Get_WhenUseDetailWithDetailSubFiltersFilters_ShouldReturnExpectedValues() {
			_memoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.BooleanValue = true;
				model.TextValue = "TextValue additional";
				model.IntegerValue = 21;
				model.FloatValue = 12.18m;
				model.DateTimeValue = DateTime.Now.AddDays(2);
				model.GuidValue = Guid.NewGuid();
				model.SysSettingsId = _sysSettingsId;
			});

			var supervisorSysAdminUnit = _memoryDataProviderMock.DataStore.AddModel<SysAdminUnit>(model => {
				model.Name = "Supervisor";
			});
			var oldShellSysSetting = _memoryDataProviderMock.DataStore.AddModel<SysSettings>(model => {
				model.Code = "UseOldShell";
				model.ValueTypeName = "Boolean";
				model.IsCacheable = true;
				model.IsPersonal = true;
			});
			_memoryDataProviderMock.DataStore.AddModel<SysSettingsValue>(model => {
				model.BooleanValue = true;
				model.TextValue = "TextValue additional";
				model.IntegerValue = 21;
				model.FloatValue = 12.18m;
				model.DateTimeValue = DateTime.Now.AddDays(2);
				model.GuidValue = Guid.NewGuid();
				model.SysSettingsId = oldShellSysSetting.Id;
				model.SysAdminUnitId = supervisorSysAdminUnit.Id;
			});

			var models = _appDataContext.Models<SysSettings>().Where(x =>
				x.IsCacheable && x.SysSettingsValues.Any(y =>
					y.BooleanValue && y.SysAdminUnit.Name == "Supervisor" &&
					y.SysSettings.SysSettingsValues.Any(z => z.IntegerValue < 22))).ToList();
			Assert.AreEqual("UseOldShell", models.FirstOrDefault()?.Code ?? string.Empty);

		}

		[Test]
		public void Get_WhenUseOrderAsc_ShouldReturnExpectedValues() {
			var sysSettingsId = SetUpSortData(DateTime.Now);
			var list = _appDataContext.Models<SysSettingsValue>().Where(x => x.SysSettingsId == sysSettingsId).OrderBy(x=>x.IntegerValue).ToList().Select(x=>x.IntegerValue).ToList();
			Assert.AreEqual(new List<int>() {22, 23, 24}, list);

		}

		[Test]
		public void Get_WhenUseOrderAscThenAsc_ShouldReturnExpectedValues() {
			var sysSettingsId = SetUpSortData(DateTime.Now);
			var list = _appDataContext.Models<SysSettingsValue>().Where(x => x.SysSettingsId == sysSettingsId)
				.OrderBy(x => x.FloatValue).ThenBy(x => x.IntegerValue).ToList().Select(x => x.IntegerValue).ToList();
			Assert.AreEqual(new List<int>() {23, 22, 24}, list);

		}

		[Test]
		public void Get_WhenUseOrderAscThenDesc_ShouldReturnExpectedValues() {
			var sysSettingsId = SetUpSortData(DateTime.Now);
			var list = _appDataContext.Models<SysSettingsValue>().Where(x => x.SysSettingsId == sysSettingsId)
				.OrderBy(x => x.FloatValue).ThenByDescending(x => x.IntegerValue).ToList().Select(x => x.IntegerValue).ToList();
			Assert.AreEqual(new List<int>() {23, 24, 22}, list);
		}

		[Test]
		public void Get_WhenUseOrderDescThenDesc_ShouldReturnExpectedValues() {
			var sysSettingsId = SetUpSortData(DateTime.Now);
			var list = _appDataContext.Models<SysSettingsValue>().Where(x => x.SysSettingsId == sysSettingsId)
				.OrderByDescending(x => x.FloatValue).ThenByDescending(x => x.IntegerValue).ToList().Select(x => x.IntegerValue).ToList();
			Assert.AreEqual(new List<int>() {24, 22, 23}, list);
		}

		[Test]
		public void Get_WhenUseTake_ShouldReturnExpectedValues() {
			var sysSettingsId = SetUpSortData(DateTime.Now);
			var list = _appDataContext.Models<SysSettingsValue>().Where(x => x.SysSettingsId == sysSettingsId)
				.OrderBy(x => x.IntegerValue).Take(2).ToList().Select(x => x.IntegerValue).ToList();
			Assert.AreEqual(new List<int>() {22, 23}, list);
		}

		[Test]
		public void Get_WhenUseSkipAndTake_ShouldReturnExpectedValues() {
			var sysSettingsId = SetUpSortData(DateTime.Now);
			var list = _appDataContext.Models<SysSettingsValue>().Where(x => x.SysSettingsId == sysSettingsId)
				.OrderBy(x => x.IntegerValue).Skip(1).Take(2).ToList().Select(x => x.IntegerValue).ToList();
			Assert.AreEqual(new List<int>() {23, 24}, list);
		}

		[Test]
		public void Get_WhenUseContainList_ShouldReturnExpectedValues() {
			var guidList = new List<Guid>() { _sysAdminUnit, Guid.NewGuid(), Guid.NewGuid() };
			var model = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => guidList.Contains(x.SysAdminUnitId));
			Assert.AreEqual(_sysAdminUnit, model?.SysAdminUnitId ?? Guid.Empty);
		}

		[Test]
		public void Get_WhenUseNotContainList_ShouldReturnExpectedValues() {
			var guidList = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() };
			var model = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => !guidList.Contains(x.SysAdminUnitId));
			Assert.AreEqual(_sysAdminUnit, model?.SysAdminUnitId ?? Guid.Empty);
		}

		[Test]
		public void Get_WhenUseStartsWith_ShouldReturnExpectedValues() {
			var model = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.TextValue.StartsWith("Text"));
			Assert.AreEqual(_sysAdminUnit, model?.SysAdminUnitId ?? Guid.Empty);
		}

		[Test]
		public void Get_WhenUseNotStartsWith_ShouldReturnExpectedValues() {
			var model = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => !x.TextValue.StartsWith("ext"));
			Assert.AreEqual(_sysAdminUnit, model?.SysAdminUnitId ?? Guid.Empty);
		}

		[Test]
		public void Get_WhenUseEndsWith_ShouldReturnExpectedValues() {
			var model = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.TextValue.EndsWith("Value"));
			Assert.AreEqual(_sysAdminUnit, model?.SysAdminUnitId ?? Guid.Empty);
		}

		[Test]
		public void Get_WhenUseNotEndsWith_ShouldReturnExpectedValues() {
			var model = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => !x.TextValue.EndsWith("Valu"));
			Assert.AreEqual(_sysAdminUnit, model?.SysAdminUnitId ?? Guid.Empty);
		}

		[Test]
		public void Get_WhenUseContains_ShouldReturnExpectedValues() {
			var model = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => x.TextValue.Contains("xtVal"));
			Assert.AreEqual(_sysAdminUnit, model?.SysAdminUnitId ?? Guid.Empty);
		}

		[Test]
		public void Get_WhenUseNotContains_ShouldReturnExpectedValues() {
			var model = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x => !x.TextValue.Contains("xtl"));
			Assert.AreEqual(_sysAdminUnit, model?.SysAdminUnitId ?? Guid.Empty);
		}

		[Test]
		public void Get_WhenUseAny_ShouldReturnExpectedValues() {
			var sysSettingsId = SetUpSortData(DateTime.Now);
			var hasRecords = _appDataContext.Models<SysSettingsValue>()
				.Any(x => x.SysSettingsId == sysSettingsId);
			Assert.AreEqual(true, hasRecords);
		}

		[Test]
		public void Get_WhenUseLongCount_ShouldReturnExpectedValues() {
			var sysSettingsId = SetUpSortData(DateTime.Now);
			var recordCount = _appDataContext.Models<SysSettingsValue>()
				.Count(x => x.SysSettingsId == sysSettingsId);
			Assert.AreEqual(3, recordCount);
		}

		[Test]
		public void Get_WhenUseShortCount_ShouldReturnExpectedValues() {
			var sysSettingsId = SetUpSortData(DateTime.Now);
			var recordCount = _appDataContext.Models<SysSettingsValue>()
				.Where(x => x.SysSettingsId == sysSettingsId).Count();
			Assert.AreEqual(3, recordCount);
		}

		[Test]
		public void Get_WhenUseSumInt_ShouldReturnExpectedValues() {
			var sysSettingsId = SetUpSortData(DateTime.Now);
			var sum = _appDataContext.Models<SysSettingsValue>()
				.Where(x => x.SysSettingsId == sysSettingsId).Sum(x=>x.IntegerValue);
			Assert.AreEqual(69, sum);
		}

		[Test]
		public void Get_WhenUseSumDecimal_ShouldReturnExpectedValues() {
			var sysSettingsId = SetUpSortData(DateTime.Now);
			var sum = _appDataContext.Models<SysSettingsValue>()
				.Where(x => x.SysSettingsId == sysSettingsId).Sum(x=>x.FloatValue);
			Assert.AreEqual(35.54m, sum);
		}

		[Test]
		public void Get_WhenUseMinInt_ShouldReturnExpectedValues() {
			var sysSettingsId = SetUpSortData(DateTime.Now);
			var min = _appDataContext.Models<SysSettingsValue>()
				.Where(x => x.SysSettingsId == sysSettingsId).Min(x=>x.IntegerValue);
			Assert.AreEqual(22, min);
		}

		[Test]
		public void Get_WhenUseMinDecimal_ShouldReturnExpectedValues() {
			var sysSettingsId = SetUpSortData(DateTime.Now);
			var min = _appDataContext.Models<SysSettingsValue>()
				.Where(x => x.SysSettingsId == sysSettingsId).Min(x=>x.FloatValue);
			Assert.AreEqual(11.18m, min);
		}

		[Test]
		public void Get_WhenUseMinDateTime_ShouldReturnExpectedValues() {
			var dt = DateTime.Now;
			var expectedValue = TrimDateTime(dt);
			var sysSettingsId = SetUpSortData(dt);
			var min = _appDataContext.Models<SysSettingsValue>()
				.Where(x => x.SysSettingsId == sysSettingsId).Min(x=>x.DateTimeValue);
			Assert.AreEqual(expectedValue, min);
		}

		[Test]
		public void Get_WhenUseMaxInt_ShouldReturnExpectedValues() {
			var sysSettingsId = SetUpSortData(DateTime.Now);
			var max = _appDataContext.Models<SysSettingsValue>()
				.Where(x => x.SysSettingsId == sysSettingsId).Max(x=>x.IntegerValue);
			Assert.AreEqual(24, max);
		}

		[Test]
		public void Get_WhenUseMaxDecimal_ShouldReturnExpectedValues() {
			var sysSettingsId = SetUpSortData(DateTime.Now);
			var max = _appDataContext.Models<SysSettingsValue>()
				.Where(x => x.SysSettingsId == sysSettingsId).Max(x=>x.FloatValue);
			Assert.AreEqual(12.18m, max);
		}

		[Test]
		public void Get_WhenUseMaxDateTime_ShouldReturnExpectedValues() {
			var dt = DateTime.Now;
			var expectedValue = TrimDateTime(dt).AddDays(2);
			var sysSettingsId = SetUpSortData(dt);
			var max = _appDataContext.Models<SysSettingsValue>()
				.Where(x => x.SysSettingsId == sysSettingsId).Max(x=>x.DateTimeValue);
			Assert.AreEqual(expectedValue, max);
		}

		[Test]
		public void CreateNewModel_ShouldReturnExpectedValues() {
			var floatValue = 20.21m;
			var booleanValue = true;
			var intValue = 10;
			var guidValue = Guid.NewGuid();
			var dateTimeValue = TrimDateTime(DateTime.Now);
			var textValue = Guid.NewGuid().ToString();
			var model = _appDataContext.CreateModel<SysSettingsValue>();
			model.SysAdminUnitId = _sysAdminUnit;
			model.FloatValue = floatValue;
			model.BooleanValue = booleanValue;
			model.IntegerValue = intValue;
			model.GuidValue = guidValue;
			model.DateTimeValue = dateTimeValue;
			model.TextValue = textValue;
			var response = _appDataContext.Save();
			Assert.AreEqual(true, response.Success);
			var createdModel = _secondAppDataContext.GetModel<SysSettingsValue>(model.Id);
			Assert.AreEqual(floatValue, createdModel.FloatValue);
			Assert.AreEqual(booleanValue, createdModel.BooleanValue);
			Assert.AreEqual(intValue, createdModel.IntegerValue);
			Assert.AreEqual(guidValue, createdModel.GuidValue);
			Assert.AreEqual(dateTimeValue, createdModel.DateTimeValue);
			Assert.AreEqual(textValue, createdModel.TextValue);
			Assert.AreEqual(_sysAdminUnit, createdModel.SysAdminUnitId);
		}

		[Test]
		public void UpdateExistedModel_ShouldReturnExpectedValues() {
			var floatValue = 30.21m;
			var booleanValue = true;
			var intValue = 20;
			var guidValue = Guid.NewGuid();
			var dateTimeValue = TrimDateTime(DateTime.Now.AddDays(-5));
			var textValue = Guid.NewGuid().ToString();
			var model = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x=>x.SysSettingsId == _sysSettingsId);
			model.SysAdminUnitId = _sysAdminUnit;
			model.FloatValue = floatValue;
			model.BooleanValue = booleanValue;
			model.IntegerValue = intValue;
			model.GuidValue = guidValue;
			model.DateTimeValue = dateTimeValue;
			model.TextValue = textValue;
			var response = _appDataContext.Save();
			Assert.AreEqual(true, response.Success);
			var updatedModel = _secondAppDataContext.GetModel<SysSettingsValue>(model.Id);
			Assert.AreEqual(floatValue, updatedModel.FloatValue);
			Assert.AreEqual(booleanValue, updatedModel.BooleanValue);
			Assert.AreEqual(intValue, updatedModel.IntegerValue);
			Assert.AreEqual(guidValue, updatedModel.GuidValue);
			Assert.AreEqual(dateTimeValue, updatedModel.DateTimeValue);
			Assert.AreEqual(textValue, updatedModel.TextValue);
			Assert.AreEqual(_sysAdminUnit, updatedModel.SysAdminUnitId);
		}

		[Test]
		public void DeleteExistedModel_ShouldReturnExpectedValues() {
			var model = _appDataContext.Models<SysSettingsValue>().FirstOrDefault(x=>x.SysSettingsId == _sysSettingsId);
			_appDataContext.DeleteModel(model);
			var response = _appDataContext.Save();
			Assert.AreEqual(true, response.Success);
			var createdModel = _secondAppDataContext.GetModel<SysSettingsValue>(model.Id);
			Assert.IsNull(createdModel);
		}

		[Test]
		public void AddModelRawData_ShouldRegisterExpectedRecords() {
			var sysSettings1Id = Guid.NewGuid();
			var sysSettings2Id = Guid.NewGuid();
			var list = new List<Dictionary<string, object>>() {
				new Dictionary<string, object>() {
					{"Id", sysSettings1Id},
					{"Name", sysSettings1Id.ToString()},
					{"Code", sysSettings1Id.ToString()}
				},
				new Dictionary<string, object>() {
					{"Id", sysSettings2Id},
					{"Name", sysSettings2Id.ToString()},
					{"Code", sysSettings2Id.ToString()}
				}
			};
			// Act
			_memoryDataProviderMock.DataStore.AddModelRawData("SysSettings", list);

			// Assert
			var model1 = _appDataContext.GetModel<SysSettings>(sysSettings1Id);
			Assert.IsNotNull(model1);
			Assert.AreEqual(sysSettings1Id.ToString(), model1.Name);
			Assert.AreEqual(sysSettings1Id.ToString(), model1.Code);
			var model2 = _appDataContext.GetModel<SysSettings>(sysSettings2Id);
			Assert.IsNotNull(model2);
			Assert.AreEqual(sysSettings2Id.ToString(), model2.Name);
			Assert.AreEqual(sysSettings2Id.ToString(), model2.Code);
		}
	}
}
