using ATF.Repository.Mapping;
using Terrasoft.Common;

namespace ATF.Repository
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.Builder;
	using ATF.Repository.Providers;
	using ATF.Repository.Queryables;

	internal class AppDataContext : IAppDataContext
	{
		private readonly IDataProvider _dataProvider;
		private readonly AppDataContextChangeTracker _changeTracker;
		private readonly ModelBuilder _modelBuilder;
		private readonly ILazyModelPropertyLoader _lazyModelPropertyLoader;

		public IChangeTracker ChangeTracker => _changeTracker;

		internal AppDataContext(IDataProvider dataProvider) {
			_changeTracker = new AppDataContextChangeTracker();
			_dataProvider = dataProvider;
			_lazyModelPropertyLoader = new LazyModelPropertyLoader(this);
			_modelBuilder = new ModelBuilder(_lazyModelPropertyLoader);
		}

		private Dictionary<string, object> GetDefaultValues<T>() where T : BaseModel {
			var schemaName = ModelUtilities.GetSchemaName(typeof(T));
			var response = _dataProvider.GetDefaultValues(schemaName);
			return response != null && response.Success
				? response.DefaultValues
				: new Dictionary<string, object>();
		}

		internal List<T> GetModelsByDataCollection<T>(List<Dictionary<string, object>> dataCollection) where T: BaseModel, new() {
			return dataCollection.Select(GetModelByValues<T>).Where(model => model != null).ToList();
		}

		private T GetModelByValues<T>(Dictionary<string, object> values) where T: BaseModel, new() {
			var id = (Guid) values["Id"];
			var trackedModel = _changeTracker.GetTrackedModel(typeof(T), id);
			if (trackedModel != null) {
				return (T)trackedModel.Model;
			}
			var model = _modelBuilder.Build<T>(values);
			_changeTracker.RegistryModel(model);
			LoadAdjectiveProperties(model);
			return model;
		}

		private void LoadAdjectiveProperties<T>(T model) where T : BaseModel, new() {
			var type = typeof(T);
			ModelMapper.GetModelItems(type).Where(x=>(x.PropertyType == ModelItemType.Lookup || x.PropertyType == ModelItemType.Detail) && !x.IsLazy).ForEach(propertyInfo => {
				_lazyModelPropertyLoader.LoadLazyProperty(model, propertyInfo);
			});
		}

		public T CreateModel<T>() where T : BaseModel, new() {
			var defaultValues = GetDefaultValues<T>();
			var model = _modelBuilder.Build<T>(defaultValues);
			model.IsNew = true;
			_changeTracker.RegistryModel(model);
			return model;
		}

		public IQueryable<T> Models<T>() where T : BaseModel, new() {
			return new ModelQuery<T>(_dataProvider, this);
		}

		public void DeleteModel<T>(T model) where T : BaseModel {
			model.IsMarkAsDeleted = true;
		}

		public ISaveResult Save() {
			var itemsToChange = _changeTracker.GetTrackedModels().Where(x => x.GetStatus() != ModelState.Unchanged)
				.OrderBy(x => x.RegisteredTime).ToList();
			var queries = itemsToChange.Select(ModifyQueryBuilder.BuildModifyQuery).Where(x => x != null)
				.ToList();

			var result = _dataProvider.BatchExecute(queries);
			if (result.Success) {
				itemsToChange.ForEach(item => {
					var state = item.GetStatus();
					if (state == ModelState.New) {
						item.Model.IsNew = false;
						((ITrackedModel)item).ReloadInitValues();
					} else if (state == ModelState.Changed) {
						((ITrackedModel)item).ReloadInitValues();
					} else if (state == ModelState.Deleted) {
						_changeTracker.UnRegistryModel(item.Model);
					}
				});
			}
			return new SaveResult() {
				Success = result.Success,
				ErrorMessage = result.ErrorMessage
			};
		}


	}


}
