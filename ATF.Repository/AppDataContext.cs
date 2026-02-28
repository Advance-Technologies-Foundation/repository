namespace ATF.Repository
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ATF.Repository.Builder;
	using ATF.Repository.Mapping;
	using ATF.Repository.Providers;
	using ATF.Repository.Queryables;
	using Terrasoft.Common;

	internal class AppDataContext : IAppDataContext
	{
		private readonly IDataProvider _dataProvider;
		private readonly AppDataContextChangeTracker _changeTracker;
		private readonly ModelBuilder _modelBuilder;
		private readonly ILazyModelPropertyManager _lazyModelPropertyManager;

		public IChangeTracker ChangeTracker => _changeTracker;

		internal AppDataContext(IDataProvider dataProvider) {
			_changeTracker = new AppDataContextChangeTracker();
			_dataProvider = dataProvider;
			_lazyModelPropertyManager = new LazyModelPropertyManager(this);
			_modelBuilder = new ModelBuilder(_lazyModelPropertyManager);
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

		private void CopyModelValues<T>(T source, T target) where T : BaseModel {
			var properties = ModelMapper.GetProperties(typeof(T));
			foreach (var property in properties) {
				var value = source.GetPropertyValue(property.PropertyName);
				target.SetPropertyValue(property.PropertyName, value);
			}

			var lookups = ModelMapper.GetLookups(typeof(T));
			foreach (var lookup in lookups) {
				var guidValue = source.GetLazyLookupKeyValue(lookup.PropertyName);
				target.SetLazyLookupKeyValue(lookup.PropertyName, guidValue);
			}
		}

		private void LoadAdjectiveProperties<T>(T model) where T : BaseModel, new() {
			var type = typeof(T);
			ModelMapper.GetModelItems(type).Where(x=>(x.PropertyType == ModelItemType.Lookup || x.PropertyType == ModelItemType.Detail) && !x.IsLazy).ForEach(propertyInfo => {
				_lazyModelPropertyManager.LoadLazyProperty(model, propertyInfo);
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

		public T GetModel<T>(Guid id) where T : BaseModel, new() {
			return Models<T>().FirstOrDefault(x => x.Id == id);
		}

		public void DeleteModel<T>(T model) where T : BaseModel {
			model.IsMarkAsDeleted = true;
		}

		public ISaveResult Save() {
			var itemsToChange = _changeTracker.GetTrackedModels().Where(x => x.GetStatus() != ModelState.Unchanged)
				.OrderBy(x => x.RegisteredTime).ToList();
			var queries = itemsToChange.Select(ModifyQueryBuilder.BuildModifyQuery).Where(x => x != null)
				.ToList();

			if (queries.Count == 0) {
				return new SaveResult() {
					Success = true,
					ErrorMessage = string.Empty
				};
			}

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
				ErrorMessage = result.Success ? result.ErrorMessage : GetErrorMessage(result)
			};
		}

		private string GetErrorMessage(IExecuteResponse result) {
			if (!string.IsNullOrWhiteSpace(result.ErrorMessage)) {
				return result.ErrorMessage;
			}
			if (result?.QueryResults != null && result.QueryResults.Any()) {
				return string.Join("\n", result.QueryResults.Select(x => x.ErrorMessage));
			}
			return "An unexpected error occurred during save operation";
		}

		public ISysSettingResponse<T> GetSysSettingValue<T>(string sysSettingCode) {
			var response = new SysSettingResponse<T>();
			try {
				response.Value = _dataProvider.GetSysSettingValue<T>(sysSettingCode);
				response.Success = true;
			} catch (Exception e) {
				response.Value = default(T);
				response.ErrorMessage = e.Message;
			}
			return response;
		}

		public IFeatureResponse GetFeatureEnabled(string featureCode) {
			var response = new FeatureResponse();
			try {
				response.Enabled = _dataProvider.GetFeatureEnabled(featureCode);
				response.Success = true;
			} catch (Exception e) {
				response.ErrorMessage = e.Message;
			}
			return response;
		}

		public IReloadResult ReloadModel<T>(T model) where T : BaseModel, new() {
			if (model.IsNew) {
				return new ReloadResult() {
					Success = false,
					ErrorMessage = "Cannot reload new model that hasn't been saved"
				};
			}

			if (model.IsMarkAsDeleted) {
				return new ReloadResult() {
					Success = false,
					ErrorMessage = "Cannot reload deleted model"
				};
			}

			try {
				var freshContext = new AppDataContext(_dataProvider);
				var freshModel = freshContext.GetModel<T>(model.Id);

				if (freshModel == null) {
					model.IsMarkAsDeleted = true;
					return new ReloadResult() {
						Success = false,
						ErrorMessage = "Model not found in database and marked as deleted"
					};
				}

				CopyModelValues(freshModel, model);

				_lazyModelPropertyManager.ClearLookupLazyValues(model);

				_changeTracker.ReloadTrackedModel(model);

				return new ReloadResult() {
					Success = true,
					ErrorMessage = null
				};
			} catch (Exception e) {
				return new ReloadResult() {
					Success = false,
					ErrorMessage = e.Message
				};
			}
		}
	}


}
