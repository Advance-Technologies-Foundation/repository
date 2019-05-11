# Введение
**ATF.Repository** - это объектно-ориентированная технология доступа к данным, является object-relational mapping (ORM) решением для **bpm`online** от **Advanced Technologies Foundation**.


Это внешняя библиотека и не является частью ядра **bpm`online**.

# Основные возможности:
- работа с данными через модели;
- построение прямых и обратных зависимостей данных через модели;
- создание изменение и удаление данных через модели с выполнением бизнес-логики.

# Репозиторий

**Repository** (*ATF.Repository.Repository*) - это хранилище и генератор моделей. Все модели должны создаваться через репозиторий. Все изменения применяются посредством репозитория. 

### Создание экземпляра репозитория:

```csharp
var repository = ClassFactory.Get<IRepository>();
repository.UserConnection = UserConnection;
```

### Сохранение изменений:

```csharp
repository.Save();
```

## Модель

**Модель** - это основная единица работы с данными. Она связывается с **Entity**.
Модель наследуется от абстрактного класса **BaseModel** (*ATF.Repository.BaseModel*).
Модель помечается атрибутом **Schema** (*ATF.Repository.Attributes.Schema*).
Свойства модели, связанные с полями **Entity** помечаются атрибутом **SchemaProperty** (*ATF.Repository.Attributes.SchemaProperty*).


**Важно!** Тип свойства должен совпадать с типом данных связанной колонки.

**Примечание**. Имя модели и свойств не обязательно должно совпадать с именем схемы и ее полей.

##### Пример:

```csharp
[Schema("TsOrderExpense")]
public class Expense : BaseModel {

	// Связь с полем типа Справочник c именем Type
	[SchemaProperty("Type")]
	public Guid TypeId { get; set; }

	// Связь с полем типа Дата-Время c именем ExpenseDate
	[SchemaProperty("ExpenseDate")]
	public DateTime ExpenseDate { get; set; }

	// Связь с полем типа Дробное c именем Amount
	[SchemaProperty("Amount")]
	public decimal Amount { get; set; }

}
```

### Настройка прямой связи

Для настройки прямой связи следует добавить в модель свойство типа модель и пометить его атрибутом **ReferenceProperty** (*ATF.Repository.Attributes.ReferenceProperty*) с указанием имени свойства модели, по которому будет производиться выборка.

##### Пример

```csharp
[Schema("TsOrderExpense")]
public class Expense : BaseModel {

	// Связь с полем типа Справочник с именем Order
	[SchemaProperty("Order")]
	public Guid OrderId { get; set; }

	// Установка прямой связи с моделью Order и связью через значение свойства модели Expense с именем OrderId
	[ReferenceProperty("OrderId")]
	public virtual Order Order { get; set; }

}

[Schema("TsOrder")]
public class Order : BaseModel {

	// Связь с полем типа Дробное c именем Amount
	[SchemaProperty("Amount")]
	public decimal Amount { get; set; }

}
```

##### Пример использования прямой связи

var amount = expenceBonus.Order.Amount;

### Настройка обратной связи

Для настройки обратной связи, следует в мастер-модели добавить свойство типа ```List<T>```, где T - модель-деталь, а в модели-детали добавить свойство типа Справочник, со ссылкой на значение, по которому определяется мастер-модель.

##### Пример
```csharp
[Schema("TsOrderExpense")]
public class Expense : BaseModel {

	// Установка обратной связи с моделью ExpenseProduct и связью через значение свойства модели ExpenseProduct с именем ExpenseId
	[DetailProperty("ExpenseId")]
	public virtual List<ExpenseProduct> ExpenseProducts { get; set; }

}

[Schema("TsOrderExpenseProduct")]
public class ExpenseProduct : BaseModel {

	// Связь с полем типа Справочник c именем Expense, для вычитки данных по обратным связям
	[SchemaProperty("Expense")]
	public Guid ExpenseId { get; set; }

	// Связь с полем типа Дробное c именем Amount
	[SchemaProperty("Amount")]
	public decimal Amount { get; set; }

}
```

##### Пример использования обратной связи

var expenseProducts = expense.ExpenseProducts.Where(x => x.Amount > 100m);

### Создание нового экземпляра модели
Создание модели происходит посредством вызова метода ```CreateItem<T>``` с указанием типа модели. При этом связанные с Entity свойства будут заполнены значениями по умолчанию.

```csharp
var bonusModel = repository.CreateItem<Bonus>();
```

### Получение модели по существующим данным из репозитория
Вычитка существующей модели происходит посредством вызова метода ```GetItem<T>``` где Id - идентификатор существующей записи.

```csharp
var bonusModel = Repository.GetItem<Bonus>(Id);
```

### Изменение данных в модели
```csharp
bonusModel.Amount = 100m;
```

### Удаление экземпляра модели из репозитория
Удаление происходит происходит посредством вызова метода ```DeleteItem<T>``` где model - экземпляр модели, которую требуется удалить.

```csharp
Repository.DeleteItem<Bonus>(model);
```

## Ленивая загрузка

Настройка моделей допускает ленивую загрузку моделей по прямым и обратным связям. Для добавление ленивой загрузки, следует добавить модификатор **virtual** к свойству. 

В указанном ниже примере, значения свойств **Order** и **Products** будут загружены сразу, а значения свойств **Document** и **Expenses** в момент первого обращения к этому свойству.

**Примечание**: Мы рекомендуем по возможности использовать ленивую загрузку.

```csharp
[Schema("TsOrderExpense")]
public class Invoice : BaseModel {

	[ReferenceProperty("DocumentId")]
	public virtual Document Document { get; set; }

	[ReferenceProperty("OrderId")]
	public Order Order { get; set; }

	[DetailProperty("InvoiceId")]
	public virtual List<Expense> Expenses { get; set; }

	[DetailProperty("InvoiceId")]
	public List<InvoiceProduct> Products { get; set; }

}
```

## Использование базовых механизмов доступа к данным

Работа с моделями не исключает использование базовых механизмов доступа к данным как через **EntitySchemaQuery** (*Terrasoft.Core.Entities.EntitySchemaQuery*) так и через **Select** (*Terrasoft.Core.DB.Select*).

В примере ниже можно увидеть использование таких подходов:

```csharp
[Schema("TsOrderExpense")]
public class Expense : BaseModel {

	public decimal BonusProductAmountSumm() {
		var select = new Select(UserConnection)
			.From("BonusProduct")
			.Column(Func.Sum("Amount"))
			.Where("Id").IsEqual(new QueryParameter(Id)) as Select;
		return select.ExecuteScalar<decimal>();
	}

	public decimal BonusProductPrimaryAmountSymm() {
		var esq = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "BonusProduct");
		var primaryAmountColumnName = esq.AddColumn(esq.CreateAggregationFunction(AggregationTypeStrict.Sum, "PrimaryAmount"));
		var collection = esq.GetEntityCollection(UserConnection);
		return collection.Count > 0
			? collection[0].GetTypedColumnValue<decimal>(primaryAmountColumnName.Name)
			: 0m;
	}
}
```
