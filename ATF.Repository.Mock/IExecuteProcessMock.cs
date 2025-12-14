namespace ATF.Repository.Mock
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Інтерфейс для налаштування mock виконання бізнес-процесу
	/// </summary>
	public interface IExecuteProcessMock : IMock
	{
		/// <summary>
		/// Вказує очікуваний вхідний параметр процесу (примітивний тип)
		/// </summary>
		/// <param name="parameterName">Ім'я параметру (BusinessProcessParameter.Name)</param>
		/// <param name="expectedValue">Очікуване значення</param>
		IExecuteProcessMock HasInputParameter(string parameterName, object expectedValue);

		/// <summary>
		/// Вказує очікуваний вхідний параметр типу List::CustomObject;
		/// </summary>
		/// <param name="parameterName">Ім'я параметру</param>
		/// <param name="expectedValue">Очікувана колекція об'єктів</param>
		IExecuteProcessMock HasInputParameter<T>(string parameterName, List<T> expectedValue) where T : class;

		/// <summary>
		/// Налаштовує успішний результат з вихідними параметрами
		/// </summary>
		/// <param name="outputParameters">Dictionary з вихідними параметрами</param>
		IExecuteProcessMock Returns(Dictionary<string, object> outputParameters);

		/// <summary>
		/// Налаштовує успішний результат з типізованим вихідним параметром типу List&lt;T&gt;
		/// </summary>
		/// <param name="parameterName">Ім'я вихідного параметру</param>
		/// <param name="outputValue">Значення для повернення</param>
		IExecuteProcessMock Returns<T>(string parameterName, List<T> outputValue) where T : class;

		/// <summary>
		/// Налаштовує результат виконання (успіх/помилка)
		/// </summary>
		/// <param name="success">true - успіх, false - помилка</param>
		/// <param name="errorMessage">Повідомлення про помилку (якщо success = false)</param>
		IExecuteProcessMock Returns(bool success, string errorMessage);

		/// <summary>
		/// Callback, який викликається при отриманні запиту
		/// </summary>
		IExecuteProcessMock ReceiveHandler(Action<IExecuteProcessMock> action);

		/// <summary>
		/// Включити/вимкнути mock (за замовчуванням true)
		/// </summary>
		bool Enabled { get; set; }

		/// <summary>
		/// Отримати фактичні серіалізовані параметри останнього виклику
		/// Використовується для assertion в тестах
		/// </summary>
		Dictionary<string, string> GetReceivedInputParameters();

		/// <summary>
		/// Отримати фактичні raw параметри останнього виклику (List::CustomObject)
		/// Використовується для assertion в тестах
		/// </summary>
		Dictionary<string, object> GetReceivedRawInputParameters();
	}
}
