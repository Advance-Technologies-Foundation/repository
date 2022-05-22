# Introduction
**ATF.Repository** - is an object-oriented data access technology. It is an object-relational mapping (ORM) solution for **Creatio** from **Advanced Technologies Foundation**.

This is an external library and not a part of **Creatio** kernel.

# General features:
- working with data via models;
- building direct and reverse data dependencies via models;
- creating, modifying and deleting data with the help of models with business logic implementation.

# Content table

- [Introduction](#introduction)
- [General features](#general-features)
- [Installation](#installation)
	- [Install as a nuget package to the project](#install-as-a-nuget-package-to-the-project)
	- [Install as Creatio-package to the Creatio-solution](#install-as-creatio-package-to-the-creatio-solution)
- [Repository](#repository)
	- [Repository instance](#repository-instance)
		- [Creating a data provider instance](#creating-a-data-provider-instance)
			- [Creating a local data provider (ATF.Repository.LocalDataProvider)](#creating-a-local-data-provider-atfrepositorylocaldataprovider)
			- [Creating a remote data provider (ATF.Repository.RemoteDataProvider)](#creating-a-remote-data-provider-atfrepositoryremotedataprovider)
		- [Creating a repository instance](#creating-a-repository-instance)
	- [Saving changes](#saving-changes)
	- [Model](#model)
		- [Direct connection setup](#direct-connection-setup)
		- [Reverse connection setup](#reverse-connection-setup)
		- [Creating a new model instance](#creating-a-new-model-instance)
		- [Receiving the model by existing data from the repository](#receiving-the-model-by-existing-data-from-the-repository)
		- [Model data changing](#model-data-changing)
		- [Deleting model instance from the repository](#deleting-model-instance-from-the-repository)
	- [Using different types of filtration](#deleting-model-instance-from-the-repository)
	- [Work with Creatio Feature Toggling](#work-with-creatio-feature-toggling)
	- [Work with Creatio System Settings](#work-with-creatio-system-settings)
- [Testing](#testing)
	- [Check model using ChangeTracker](#check-model-using-ChangeTracker)
	- [Using mocking data provider](#using-mocking-data-provider)
		- [Mocking Model default values](#mocking-model-default-values)
		- [Mocking Get Models result](#mocking-get-models-result)
		- [Mocking Scalar result](#mocking-scalar-result)
		- [Mocking Save result](#mocking-save-result)

# Installation

## Install as a nuget package to the project

1. Open a command line and switch to the directory that contains your project file.

2. Use the following command to install a NuGet package:

```dotnetcli
dotnet add package ATF.Repository
```

3. After the command completes, look at the project file to make sure the package was installed.

You can open the `.csproj` file to see the added reference:

```xml
<ItemGroup>
  <PackageReference Include="ATF.Repository" Version="2.0.6" />
</ItemGroup>
```

## Install as Creatio-package to the Creatio-solution

1. Open a command line and switch to the directory that contains `ATF_Repository.gz`.

2. Use the following command to install ATF.Repository package by clio:

```dotnetcli
clio push-pkg ATF_Repository.gz
```

3. After the command completes, look at `WorkspaceExplorer` to make sure the package was installed.

# Repository

**ATF.Repository.IAppDataContext** - is a storage and model generator. All models should be created via the repository. All changes are applied via the repository. 

## Repository instance:
For creating repository instance we have to create **DataProvider** as repository data source.

### Creating a data provider instance:
We can, depending on our needs, create either a local or a remote data provider.

#### Creating a local data provider (*ATF.Repository.LocalDataProvider*):

```csharp
IDataProvider localDataProvider = new LocalDataProvider(UserConnection);
```
#### Creating a remote data provider (*ATF.Repository.RemoteDataProvider*):

```csharp
string url = "https://site.url";
string login = "Login"; // Creatio User Login
string password = "Password"; // Creatio User Password
IDataProvider remoteDataProvider = new RemoteDataProvider(url, login, password);
```

### Creating a repository instance:
For creating repository instance we should use *AppDataContextFactory.GetAppDataContext*.
```csharp
var appDataContext = AppDataContextFactory.GetAppDataContext(dataProvider);
```

## Saving changes:

```csharp
appDataContext.Save();
```

## Model

**Model** - basic unit of data modeling. It is connected to the **Entity**.
The model is inherited from an abstract **BaseModel** class (*ATF.Repository.BaseModel*).
It is marked with the **Schema** attribute (*ATF.Repository.Attributes.Schema*).
Model properties, connected to the **Entity** fields, are marked with **SchemaProperty** attribute (*ATF.Repository.Attributes.SchemaProperty*).


**Attention!** The type of data of property must be the same as the type of data in connected column.

**Note**. It is not required that the title of the model and its properties matches the title of the schema and its fields.

##### Example:

```csharp
[Schema("TsOrderExpense")]
public class Expense : BaseModel {

	// Connection with the "Type" lookup field
	[SchemaProperty("Type")]
	public Guid TypeId { get; set; }

	// Connection with the "ExpenseDate" Date-Time field 
	[SchemaProperty("ExpenseDate")]
	public DateTime ExpenseDate { get; set; }

	// Connection with the "Amount" decimal field 
	[SchemaProperty("Amount")]
	public decimal Amount { get; set; }

}
```

### Direct connection setup

To set up direct connection, add a property of a model type to the model and mark it with the **LookupProperty** attribute (*ATF.Repository.Attributes.LookupProperty*).

**Attention!** For dirrect connection, we should always use key word *virtual*.

##### Example:

```csharp
[Schema("TsOrderExpense")]
public class Expense : BaseModel {

	// Connection with the "Order" lookup field
	[SchemaProperty("Order")]
	public Guid OrderId { get; set; }

	// Setting up direct connection with the Order model, using the value of "Order" lookup field
	[LookupProperty("Order")]
	public virtual Order Order { get; set; }

}

[Schema("TsOrder")]
public class Order : BaseModel {

	// Connection with the "Amount" Decimal field
	[SchemaProperty("Amount")]
	public decimal Amount { get; set; }

}
```

##### Example of using direct connection

```csharp
var amount = expenceBonus.Order.Amount;
```

### Reverse connection setup

To set up reverse connection, add a property of ```List<T>``` type to a master model, where "T" states for a detail model. As argument for *DetailProperty* attribute we should use name of connected property from detail model.

**Attention!** For reverse connection, we should always use key word *virtual*.

##### Example:

```csharp
[Schema("TsOrderExpense")]
public class Expense : BaseModel {

	// Setting up reverse connection with the ExpenseProduct model, using the name of detail entity schema column for link.
	[DetailProperty("TsOrderExpenseId")]
	public virtual List<ExpenseProduct> ExpenseProducts { get; set; }

}

[Schema("TsOrderExpenseProduct")]
public class ExpenseProduct : BaseModel {

	//Entity schema TsOrderExpenseProduct contain column TsOrderExpense
	//but we shouldn't add property with map on that column if we want to create reverse connection

	// Connection with the "Amount" Decimal field
	[SchemaProperty("Amount")]
	public decimal Amount { get; set; }
	
	// Connected with Expense model by that property
	[SchemaProperty("TsOrderExpenseId")]
	public Guid TsOrderExpenseId { get; set; }

}
```

##### Reverce connection use case

```csharp
var expenseProducts = expense.ExpenseProducts.Where(x => x.Amount > 100m);
```

### Creating a new model instance

A model is created by calling a ```CreateItem<T>``` method and specifying the model type. Upon that, properties, connected to the Entity, will be populated with default values.

```csharp
var bonusModel = appDataContext.CreateItem<Bonus>();
```

### Receiving the model by existing data from the repository

Existing model is read by means of calling a ```GetItem<T>``` method, where Id - is the identifier of the existing record.

```csharp
var bonusModel = appDataContext.GetItem<Bonus>(Id);
```

### Model data changing

```csharp
bonusModel.Amount = 100m;
```

### Deleting model instance from the repository

Model instance is deleted by calling ```DeleteItem<T>``` method, where  model - is the instance to be deleted.

```csharp
appDataContext.DeleteItem<Bonus>(model);
```

## Using different types of filtration

### Load models where Age greater or equal 50

```csharp
var models = appDataContext.Models<Contact>().Where(x => x.Age >= 50 ).ToList();
```

### Load models where Active is true

```csharp
var models = appDataContext.Models<Contact>().Where(x => x.Active).ToList();
or
var models = appDataContext.Models<Contact>().Where(x => x.Active == true).ToList();
```

### Load Top 10, Skip 20 models where Name contains substring and order by CreatedOn

```csharp
var models = appDataContext.Models<Contact>().Take(10).Skip(20).Where(x => x.Name.Contains("Abc"))
	.OrderBy(x => x.CreatedOn).ToList();
```

### Load models with some conditions

```csharp
var models = appDataContext.Models<Contact>().Where(x => x.Age > 10)
	.Where(x => x.TypeId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef")).ToList();
```

### Load first model with some conditions and orders

```csharp
var model = appDataContext.Models<Contact>().Where(x => x.Age > 10)
	.Where(x => x.TypeId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef")).OrderBy(x => x.Age)
	.ThenByDescending(x => x.Name).FirstOrDefault();
```

### Load sum by the column with some conditions

```csharp
var age = appDataContext.Models<Contact>().Where(x => x.Age > 10)
	.Where(x => x.TypeId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef")).Sum(x=>x.Age);
```

### Load count by the column with some conditions

```csharp
var age = appDataContext.Models<Contact>().Where(x => x.Age > 10)
	.Count(x => x.TypeId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef"));
```

### Load Max by the column with some conditions

```csharp
var maxAge = appDataContext.Models<Contact>().Where(x => x.Age > 10)
	.Where(x => x.TypeId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef")).Max(x=>x.Age);
```

### Load Min by the column with some conditions

```csharp
var minAge = appDataContext.Models<Contact>().Where(x => x.Age > 10)
	.Where(x => x.TypeId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef")).Min(x=>x.Age);
```

### Load Average by the column with some conditions

```csharp
var minAge = appDataContext.Models<Contact>().Where(x => x.Age > 10)
	.Where(x => x.TypeId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef")).Average(x=>x.Age);
```

### Load records with conditions in detail models

```csharp
var model = appDataContext.Models<Contact>().Where(x =>
	x.ContactInTags.Any(y => y.TagId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef"))).ToList();
```

### Load records with conditions in detail models and inner detail models

```csharp
var models = appDataContext.Models<Account>().Where(x =>
	x.Contacts.Where(y=>y.ContactInTags.Any(z => z.TagId == new Guid("ee98ccf4-fb0d-47d1-a143-fc1468e73cef")))).ToList();
```

## Work with Creatio Feature Toggling

*Feature toggle* is a software development technique that enables connecting additional features to a live application. This lets you use continuous integration while preserving the working capacity of the application and hiding features you are still developing.

To check the feature state you can use `GetFeatureEnabled` method. 

```csharp
var response = appDataContext.GetFeatureEnabled("FeatureCode");
if (!response.Success) {
	throw new Exception($"Cannot get feature state: {response.ErrorMessage}");
}
if (response.Value) {
	// do something
}

```
## Work with Creatio System Settings

*The Creatio System settings* contains all global system settings that used in all functional blocks.
To get System setting values you can use `GetSysSettingValue` method.

**Attention!** The type of system setting data value must be the same as the type of data in Creatio.

```csharp
var response = appDataContext.GetSysSettingValue<int>("LimitInMinutes")
if (!response.Success) {
	throw new Exception($"Cannot get feature state: {response.ErrorMessage}");
}
var limitInMinutes = response.Value;

```

# Testing

Testing it is an important part of the full development cycle. To be sure your application works correctly, you can write tests with using one of two testing strategies: tracking of Model changes or using mocking data provider. 

## Check models using ChangeTracker

To track model changes you can use `GetTrackedModel` or `GetTrackedModels` methods. 
Both of methods returns implementation of public interface `ITrackedModel`. 
In most simple cases `ChangeTracker` can help you with checking model status and other simple parameters.

```csharp
public interface ITrackedModel<out T> where T: BaseModel
{
	// Tracked model
	T Model { get; }

	// Tracked model type nested from BaseModel
	Type Type { get; }

	// The time when model registred in data context.
	DateTime RegisteredTime { get; }

	// Model status
	// - New - new Model that not yet saved
	// - Changed - existed Model whose changed values not yet saved
	// - Unchanged - existed Model wishout any changes
	// - Deleted - existed Model that marked as deleted.
	ModelState GetStatus();

	// The doctionary of changing values of Model.
	Dictionary<string, object> GetChanges();
}

```

### Get TrackedModel using Model

```csharp
Bonus bonusModel = appDataContext.CreateItem<Bonus>();

// Assert
var trackedModel = appDataContext.ChangeTracker.GetTrackedModel(bonusModel);

var changedValues = trackedModel.GetChanges();
```

### Get list of all TrackedModels

```csharp
var trackedModels = appDataContext.ChangeTracker.GetTrackedModels();
var bonusCount = trackedModels.Count(x => x.Type == typeof(Bonus));
```

### Get list of typed TrackedModels

```csharp
var trackedModels = appDataContext.ChangeTracker.GetTrackedModels<Bonus>();

var newCount = trackedModels.Count(x => x.GetStatus() == ModelState.New);
```

## Use mocking data provider

Another way to be sure your application works correctly is testing your solution using mocking data provider. To use that way you have to use new nuget package `ATF.Repository.Mock` (https://www.nuget.org/packages/ATF.Repository.Mock).

`ATF.Repository.Mock` is a powerful tool for mocking all interactions with data.

First you need to install that package to your unit-test project.

```dotnetcli
dotnet add package ATF.Repository.Mock
```
Then you will be able to use `DataProviderMock` class.

```csharp
var dataProviderMock = new DataProviderMock();
var appDataContext = AppDataContextFactory.GetAppDataContext(dataProviderMock);
```

### Mocking Model default values

When you create Model, ATF.Repository trying get default values of EntitySchema columns for that model.
If you want to mock these default values you can use `MockDefaultValues` method.

```csharp
// Mock default values
var mock = dataProviderMock.MockDefaultValues("Invoice").Returns(new Dictionary<string, object>() {
	{"Number", "New"},
	{"MinAmount", 10.00m}
});

// Create new Model instance
var model = appDataContext.CreateModel<InvoiceModel>();
//model.MinAmount - will be equal 10.00m

// Subscribe to call mock. That arrow method will be called every time when call default vaules for that Model.
var countOfCalling = 0;
mock.ReceiveHandler(x => {
	countOfCalling++;
});

// Simple counter of using that mock.
mock.ReceivedCount
```

### Mocking Get Models result

When you need to mock result of `Models<T>` method, you can use `MockItems` that return implement of public interface `IItemsMock`. 

```csharp
// Create mock for Models on Model that based on Invoice Entity Schema
var mock = dataProviderMock.MockItems("Invoice");
// The filter of GetImems lamda expression must have string 'ExpectedOrderNumber' as filter value.
mock.FilterHas("ExpectedOrderNumber");
// Also the filter of GetImems lamda expression must have decimal 5m as filter value.
mock.FilterHas(5m);
// If frevious conditions are passed, mock will return that list of dictionaries as values for Models.
mock.Returns(new List<Dictionary<string, object>>() {
	new Dictionary<string, object>() {
		{"Id", Guid.NewGuid()},
		{"Number", "OX-00101/115-2"},
		{"Amount", 11.00m}
	}
});

// Subscribe to call mock. That arrow method will be called every time when mock conditions passed.
var countOfCalling = 0;
mock.ReceiveHandler(x => {
	countOfCalling++;
});

// Simple counter of using that mock.
mock.ReceivedCount

// Then calling of Models<T> with expression will result mocked data
var models = appDataContext.Models<InvoiceModel>().Where(x => x.Order.Number == "ExpectedOrderNumber" && x.Amount > 5m).ToList();
// models.Count == 1
// models.First.Amount == 11.00m

```

### Mocking Scalar result

When you need to mock scalar result of `Models<T>` method, you can use `MockScalar` that returns implement of public interface `IScalarMock`. 

```csharp
//  Create scalar mock for Models on Model that based on Invoice Entity Schema with filter use 10m and 50m and lamda expression return scalar value. 
var mock = dataProviderMock
	.MockScalar("Invoice", AggregationScalarType.Avg)
	.FilterHas(10m)
	.FilterHas(50m)
	.Returns(15.5m);

// Subscribe to call mock. That arrow method will be called every time when mock conditions passed.
var countOfCalling = 0;
mock.ReceiveHandler(x => {
	countOfCalling++;
});

// Simple counter of using that mock.
mock.ReceivedCount

var averageAmount = appDataContext.Models<InvoiceModel>().Where(x => x.Amount > 10m && x.Amount <= 50m).Average(x=>x.Amount);

// averageAmount == 15.5m
```

### Mocking Save result

When you need to mock Save result or check if call Insert/Update/Delete Model with expected condition or changed column values, you can use `MockSavingItem` method that returns implement of public interface `IMockSavingItem`. 

#### Checking if Repository call Insert model action

To check, if repository called Insert you can register mock on that operation.
For example:

```csharp

//Check if call Insert operation with Model based on Invoice entity
var mock = dataProviderMock.MockSavingItem("Invoice", SavingOperation.Insert);

// Number property for that Model should equal "AX-005-10"
mock.ChangedValueHas("Number", "AX-005-10");

// Amount property for that Model should equal 10.15m
mock.ChangedValueHas("Amount", 10.15m)

// One of properties for that Model should equal true
mock.ChangedValueHas(true);

// Create new Model instance
var model = appDataContext.CreateModel<InvoiceModel>();
model.Number = "AX-005-10";
model.Amount = 10.15m;
model.IsClosed = true;

// Save repository data
appDataContext.Save();


// Simple counter of using that mock.
mock.ReceivedCount

// Subscribe to call mock. That arrow method will be called every time when mock conditions passed.
var countOfCalling = 0;
mock.ReceiveHandler(x => {
	countOfCalling++;
});
```

#### Checking if Repository call Update Model action

Every updating Model call Update action with filter by primary property value, that is why we should use filter by Id for check if repository call Update action for the Model.

```csharp
var invoiceId = Guid.NewGuid();
var updatingMock = dataProviderMock.MockSavingItem("Invoice", SavingOperation.Update)
	.ChangedValueHas(12.10m)
	.ChangedValueHas(false)
	.FilterHas(invoiceId);

// Act
var model = appDataContext.Models<InvoiceModel>().First(x => x.Id == invoiceId);
model.Amount = 12.10m;
model.IsClosed = false;

appDataContext.Save();

// Simple counter of using that mock. Property will equal 1 if Repository calls Update action for Model whith properties equal expected values.
mock.ReceivedCount

// Subscribe to call mock. That arrow method will be called every time when mock conditions passed.
var countOfCalling = 0;
mock.ReceiveHandler(x => {
	countOfCalling++;
});
```

#### Checking if Repository call Delete Model action

```csharp
var invoiceId = Guid.NewGuid();
var deletingMock = dataProviderMock.MockSavingItem("Invoice", SavingOperation.Delete)
	.FilterHas(invoiceId);

// Act
var models = appDataContext.Models<InvoiceModel>().Where(x => x.Id == invoiceId).ToList();
var model = models.First();
appDataContext.DeleteModel(model);

appDataContext.Save();

// Simple counter of using that mock. Property will equal 1 if Repository calls Delete action for Model whith Id equal expected value;
mock.ReceivedCount

// Subscribe to call mock. That arrow method will be called every time when mock conditions passed.
var countOfCalling = 0;
mock.ReceiveHandler(x => {
	countOfCalling++;
});
```

### Mocking System setting value

```csharp
dataProviderMock.MockSysSettingValue("SystemSettingsCode", 180);

var response = _appDataContext.GetSysSettingValue<int>("SystemSettingsCode");

// response.Value will be equal 180;
```

### Mocking Feature status

```csharp
dataProviderMock.MockFeatureEnable("FirstFeature", true);
dataProviderMock.MockFeatureEnable("SecondFeature", false);

var firstResponse = appDataContext.GetFeatureEnabled("FirstFeature");
// firstResponse.Enabled will be equal true;

var secondResponse = appDataContext.GetFeatureEnabled("SecondFeature");
// secondResponse.Enabled will be equal false;
```