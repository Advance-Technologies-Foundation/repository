namespace ATF.Repository.UnitTests
{
	using ATF.Repository.Providers;
	using ATF.Repository.UnitTests.Models;
	using NSubstitute;
	using NUnit.Framework;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	#region Class: AppDataContextReloadTests

	[TestFixture]
	public class AppDataContextReloadTests
	{
		#region Fields: Private

		private IDataProvider _dataProvider;
		private IAppDataContext _appDataContext;

		#endregion

		#region Methods: Public

		[SetUp]
		public void SetUp() {
			_dataProvider = Substitute.For<IDataProvider>();
			_appDataContext = AppDataContextFactory.GetAppDataContext(_dataProvider);
		}

		[Test]
		public void ReloadModel_ExistingModel_Success() {
			// Arrange
			var modelId = Guid.NewGuid();
			var initialName = "Initial Name";
			var updatedName = "Updated Name";
			var initialLookupId = Guid.NewGuid();
			var updatedLookupId = Guid.NewGuid();

			// Створюємо модель через GetModel (імітуємо існуючу модель)
			_dataProvider.GetItems(Arg.Any<ISelectQuery>())
				.Returns(new ItemsResponse() {
					Success = true,
					Items = new List<Dictionary<string, object>>() {
						new Dictionary<string, object>() {
							{"Id", modelId},
							{"StringValue", initialName},
							{"LookupValue", initialLookupId}
						}
					}
				});

			var model = _appDataContext.GetModel<TypedTestModel>(modelId);
			Assert.IsNotNull(model);
			Assert.AreEqual(initialName, model.StringValue);

			// Змінюємо значення моделі
			model.StringValue = "Modified Name";

			// Тепер при reload повертаємо оновлені дані
			_dataProvider.GetItems(Arg.Any<ISelectQuery>())
				.Returns(new ItemsResponse() {
					Success = true,
					Items = new List<Dictionary<string, object>>() {
						new Dictionary<string, object>() {
							{"Id", modelId},
							{"StringValue", updatedName},
							{"LookupValue", updatedLookupId}
						}
					}
				});

			// Act
			var result = _appDataContext.ReloadModel(model);

			// Assert
			Assert.IsTrue(result.Success);
			Assert.IsNull(result.ErrorMessage);
			Assert.AreEqual(updatedName, model.StringValue, "Model should have updated values from DB");
			Assert.IsFalse(model.IsNew);
			Assert.IsFalse(model.IsMarkAsDeleted);
		}

		[Test]
		public void ReloadModel_NewModel_ReturnsFalse() {
			// Arrange
			_dataProvider.GetDefaultValues("TypedTestModel").Returns(new DefaultValuesResponse() {
				Success = true,
				DefaultValues = new Dictionary<string, object>()
			});

			var newModel = _appDataContext.CreateModel<TypedTestModel>();
			newModel.StringValue = "New Model";

			// Act
			var result = _appDataContext.ReloadModel(newModel);

			// Assert
			Assert.IsFalse(result.Success);
			Assert.IsNotNull(result.ErrorMessage);
			Assert.That(result.ErrorMessage, Does.Contain("new model").IgnoreCase);
			Assert.AreEqual("New Model", newModel.StringValue, "Model should not be changed");
			Assert.IsTrue(newModel.IsNew);
		}

		[Test]
		public void ReloadModel_DeletedModel_ReturnsFalse() {
			// Arrange
			var modelId = Guid.NewGuid();
			_dataProvider.GetItems(Arg.Any<ISelectQuery>())
				.Returns(new ItemsResponse() {
					Success = true,
					Items = new List<Dictionary<string, object>>() {
						new Dictionary<string, object>() {
							{"Id", modelId},
							{"StringValue", "Test"}
						}
					}
				});

			var model = _appDataContext.GetModel<TypedTestModel>(modelId);
			_appDataContext.DeleteModel(model);

			// Act
			var result = _appDataContext.ReloadModel(model);

			// Assert
			Assert.IsFalse(result.Success);
			Assert.IsNotNull(result.ErrorMessage);
			Assert.That(result.ErrorMessage, Does.Contain("deleted").IgnoreCase);
			Assert.IsTrue(model.IsMarkAsDeleted);
		}

		[Test]
		public void ReloadModel_ModelNotFoundInDB_MarksAsDeleted() {
			// Arrange
			var modelId = Guid.NewGuid();

			// Спочатку модель існує
			_dataProvider.GetItems(Arg.Any<ISelectQuery>())
				.Returns(new ItemsResponse() {
					Success = true,
					Items = new List<Dictionary<string, object>>() {
						new Dictionary<string, object>() {
							{"Id", modelId},
							{"StringValue", "Test"}
						}
					}
				});

			var model = _appDataContext.GetModel<TypedTestModel>(modelId);
			Assert.IsNotNull(model);

			// Потім модель видаляється з БД (повертаємо порожній результат)
			_dataProvider.GetItems(Arg.Any<ISelectQuery>())
				.Returns(new ItemsResponse() {
					Success = true,
					Items = new List<Dictionary<string, object>>()
				});

			// Act
			var result = _appDataContext.ReloadModel(model);

			// Assert
			Assert.IsFalse(result.Success);
			Assert.IsNotNull(result.ErrorMessage);
			Assert.That(result.ErrorMessage, Does.Contain("not found").IgnoreCase);
			Assert.IsTrue(model.IsMarkAsDeleted, "Model should be marked as deleted when not found in DB");
		}

		[Test]
		public void ReloadModel_ClearsLazyLookups_PreservesDetails() {
			// Arrange
			var modelId = Guid.NewGuid();
			var lookupId = Guid.NewGuid();
			var lookupName = "Lookup Name";

			// Створюємо модель
			_dataProvider.GetItems(Arg.Any<ISelectQuery>())
				.Returns(
					// Головна модель
					new ItemsResponse() {
						Success = true,
						Items = new List<Dictionary<string, object>>() {
							new Dictionary<string, object>() {
								{"Id", modelId},
								{"StringValue", "Test"},
								{"LookupValue", lookupId}
							}
						}
					},
					// Lookup модель
					new ItemsResponse() {
						Success = true,
						Items = new List<Dictionary<string, object>>() {
							new Dictionary<string, object>() {
								{"Id", lookupId},
								{"Name", lookupName}
							}
						}
					}
				);

			var model = _appDataContext.GetModel<TypedTestModel>(modelId);

			// Завантажуємо lazy lookup property
			var lookup = model.LookupValue;
			Assert.IsNotNull(lookup);
			Assert.AreEqual(lookupName, lookup.Name);

			// Переконуємось що lookup в LazyValues
			var lookupKey = model.GetLazyLookupKey("LookupValue");
			Assert.IsTrue(model.LazyValues.ContainsKey(lookupKey), "Lookup Guid should be in LazyValues");
			Assert.IsTrue(model.LazyValues.ContainsKey("LookupValue"), "Lookup model should be in LazyValues");

			// Також додаємо detail property в LazyValues (імітація завантаженого detail)
			var detailList = new List<TypedTestModel>();
			model.LazyValues["DetailModels"] = detailList;

			// Reload повертає ті самі дані
			_dataProvider.GetItems(Arg.Any<ISelectQuery>())
				.Returns(new ItemsResponse() {
					Success = true,
					Items = new List<Dictionary<string, object>>() {
						new Dictionary<string, object>() {
							{"Id", modelId},
							{"StringValue", "Test"},
							{"LookupValue", lookupId}
						}
					}
				});

			// Act
			var result = _appDataContext.ReloadModel(model);

			// Assert
			Assert.IsTrue(result.Success);
			Assert.IsFalse(model.LazyValues.ContainsKey(lookupKey), "Lookup Guid should be cleared from LazyValues");
			Assert.IsFalse(model.LazyValues.ContainsKey("LookupValue"), "Lookup model should be cleared from LazyValues");
			Assert.IsTrue(model.LazyValues.ContainsKey("DetailModels"), "Detail property should NOT be cleared from LazyValues");
			Assert.AreEqual(detailList, model.LazyValues["DetailModels"], "Detail property value should be preserved");
		}

		[Test]
		public void ReloadModel_ModelStateBecomesUnchanged() {
			// Arrange
			var modelId = Guid.NewGuid();
			var initialValue = "Initial";

			_dataProvider.GetItems(Arg.Any<ISelectQuery>())
				.Returns(new ItemsResponse() {
					Success = true,
					Items = new List<Dictionary<string, object>>() {
						new Dictionary<string, object>() {
							{"Id", modelId},
							{"StringValue", initialValue}
						}
					}
				});

			var model = _appDataContext.GetModel<TypedTestModel>(modelId);

			// Змінюємо модель
			model.StringValue = "Modified";

			// Перевіряємо що модель має зміни
			var trackedModel = _appDataContext.ChangeTracker.GetTrackedModel(model);
			Assert.AreEqual(ModelState.Changed, trackedModel.GetStatus(), "Model should be in Changed state before reload");

			// Reload повертає оригінальні дані
			_dataProvider.GetItems(Arg.Any<ISelectQuery>())
				.Returns(new ItemsResponse() {
					Success = true,
					Items = new List<Dictionary<string, object>>() {
						new Dictionary<string, object>() {
							{"Id", modelId},
							{"StringValue", initialValue}
						}
					}
				});

			// Act
			var result = _appDataContext.ReloadModel(model);

			// Assert
			Assert.IsTrue(result.Success);
			Assert.AreEqual(ModelState.Unchanged, trackedModel.GetStatus(), "Model must be in Unchanged state after reload");
		}

		[Test]
		public void ReloadModel_ResetsTrackedChanges() {
			// Arrange
			var modelId = Guid.NewGuid();
			var initialValue = "Initial";

			_dataProvider.GetItems(Arg.Any<ISelectQuery>())
				.Returns(new ItemsResponse() {
					Success = true,
					Items = new List<Dictionary<string, object>>() {
						new Dictionary<string, object>() {
							{"Id", modelId},
							{"StringValue", initialValue}
						}
					}
				});

			var model = _appDataContext.GetModel<TypedTestModel>(modelId);

			// Змінюємо модель
			model.StringValue = "Modified";

			// Перевіряємо що модель tracked і має зміни
			var trackedModel = _appDataContext.ChangeTracker.GetTrackedModel(model);
			Assert.IsNotNull(trackedModel);
			var changesBefore = trackedModel.GetChanges();
			Assert.IsTrue(changesBefore.Any(), "Model should have changes before reload");

			// Reload повертає оригінальні дані
			_dataProvider.GetItems(Arg.Any<ISelectQuery>())
				.Returns(new ItemsResponse() {
					Success = true,
					Items = new List<Dictionary<string, object>>() {
						new Dictionary<string, object>() {
							{"Id", modelId},
							{"StringValue", initialValue}
						}
					}
				});

			// Act
			var result = _appDataContext.ReloadModel(model);

			// Assert
			Assert.IsTrue(result.Success);
			Assert.AreEqual(initialValue, model.StringValue);

			var changesAfter = trackedModel.GetChanges();
			Assert.IsFalse(changesAfter.Any(), "Model should have no changes after reload");
			Assert.AreEqual(ModelState.Unchanged, trackedModel.GetStatus(), "Model status should be Unchanged");
		}

		[Test]
		public void ReloadModel_PreservesModelReference() {
			// Arrange
			var modelId = Guid.NewGuid();

			_dataProvider.GetItems(Arg.Any<ISelectQuery>())
				.Returns(new ItemsResponse() {
					Success = true,
					Items = new List<Dictionary<string, object>>() {
						new Dictionary<string, object>() {
							{"Id", modelId},
							{"StringValue", "Initial"}
						}
					}
				});

			var model = _appDataContext.GetModel<TypedTestModel>(modelId);
			var originalReference = model;

			// Reload
			_dataProvider.GetItems(Arg.Any<ISelectQuery>())
				.Returns(new ItemsResponse() {
					Success = true,
					Items = new List<Dictionary<string, object>>() {
						new Dictionary<string, object>() {
							{"Id", modelId},
							{"StringValue", "Updated"}
						}
					}
				});

			// Act
			var result = _appDataContext.ReloadModel(model);

			// Assert
			Assert.IsTrue(result.Success);
			Assert.AreSame(originalReference, model, "Model reference should be the same object");
			Assert.AreEqual("Updated", model.StringValue);
		}

		[Test]
		public void ReloadModel_DoesNotClearDetailProperties() {
			// Arrange
			var modelId = Guid.NewGuid();

			_dataProvider.GetItems(Arg.Any<ISelectQuery>())
				.Returns(new ItemsResponse() {
					Success = true,
					Items = new List<Dictionary<string, object>>() {
						new Dictionary<string, object>() {
							{"Id", modelId},
							{"StringValue", "Test"}
						}
					}
				});

			var model = _appDataContext.GetModel<TypedTestModel>(modelId);

			// Додаємо detail properties в LazyValues
			var detailModels = new List<TypedTestModel>();
			var anotherDetailModels = new List<TypedTestModel>();
			model.LazyValues["DetailModels"] = detailModels;
			model.LazyValues["AnotherDetailModels"] = anotherDetailModels;

			// Reload
			_dataProvider.GetItems(Arg.Any<ISelectQuery>())
				.Returns(new ItemsResponse() {
					Success = true,
					Items = new List<Dictionary<string, object>>() {
						new Dictionary<string, object>() {
							{"Id", modelId},
							{"StringValue", "Test"}
						}
					}
				});

			// Act
			var result = _appDataContext.ReloadModel(model);

			// Assert
			Assert.IsTrue(result.Success);
			Assert.IsTrue(model.LazyValues.ContainsKey("DetailModels"), "DetailModels should still be in LazyValues");
			Assert.IsTrue(model.LazyValues.ContainsKey("AnotherDetailModels"), "AnotherDetailModels should still be in LazyValues");
			Assert.AreSame(detailModels, model.LazyValues["DetailModels"], "DetailModels reference should be preserved");
			Assert.AreSame(anotherDetailModels, model.LazyValues["AnotherDetailModels"], "AnotherDetailModels reference should be preserved");
		}

		#endregion
	}

	#endregion
}
