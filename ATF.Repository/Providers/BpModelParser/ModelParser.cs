using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ATF.Repository.Attributes;
using Castle.Components.DictionaryAdapter.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terrasoft.Core.ServiceModelContract;

namespace ATF.Repository.Providers.BpModelParser {
	
	/// <summary>
	///  Provides static helper methods for parsing JSON data and converting it into strongly-typed
	///  business process models. The parser maps process parameter values from the JSON response
	///  based on the attributes and properties defined in the target model class, allowing the creation
	///  of populated instances of a specified generic type.
	/// </summary>
	/// <remarks>
	///  This utility class serves as a bridge between serialized JSON responses from services
	///  and the corresponding domain-specific model objects, following conventions established by
	///  attributes like <see cref="ProcessParameterAttribute" />.
	/// </remarks>
	public static class ModelParser {

		#region Fields: Private

		/// <summary>
		///  Converts a value to the specified target type using appropriate type conversion methods.
		/// </summary>
		/// <param name="type">The target type to convert the value to.</param>
		/// <returns>
		///  The converted value of the specified type, or null if:
		///  <list type="bullet">
		///   <li>The input value is null</li>
		///   <li>The conversion fails</li>
		///  </list>
		/// </returns>
		/// <remarks>
		///  This method handles:
		///  <list type="bullet">
		///   <li>Nullable types by extracting their underlying type</li>
		///   <li>Primitive types using registered type converters</li>
		///   <li>String conversion using ToString()</li>
		///   <li>Other types using Convert.ChangeType with invariant culture</li>
		///  </list>
		/// </remarks>
		private static Func<object, object> GetTypeConverters(Type type) {
			var converters = new Dictionary<Type, Func<object, object>> {
				{typeof(int), value => int.TryParse(value?.ToString(), NumberStyles.Integer,CultureInfo.InvariantCulture, out int result) ? (object)result : null},
				{typeof(decimal), value => decimal.TryParse(value?.ToString(), NumberStyles.AllowDecimalPoint ,CultureInfo.InvariantCulture, out decimal result) ? (object)result : null},
				{typeof(float), value => float.TryParse(value?.ToString(), out float result) ? (object)result : null},
				{typeof(double), value => double.TryParse(value?.ToString(), out double result) ? (object)result : null},
				{typeof(bool), value => bool.TryParse(value?.ToString(), out bool result) ? (object)result : null},
				{typeof(long), value => long.TryParse(value?.ToString(), out long result) ? (object)result : null},
				{typeof(ulong), value => ulong.TryParse(value?.ToString(), out ulong result) ? (object)result : null},
				{typeof(Guid), value => Guid.TryParse(value?.ToString(), out Guid result) ? (object)result : null},
				{typeof(DateTime), value => DateTime.TryParse(value?.ToString(), CultureInfo.InvariantCulture, 
					DateTimeStyles.AssumeUniversal| DateTimeStyles.AdjustToUniversal, out DateTime result) ? (object)result : null}
			};
			return converters.TryGetValue(type, out Func<object, object> converter) ? converter : null;
		}
		
		#endregion

		#region Methods: Private

		/// <summary>
		///  Converts a value to the specified type using predefined type converters or general conversion methods.
		/// </summary>
		/// <param name="type">The target type to convert the value to. Can be a nullable type.</param>
		/// <param name="value">The value to be converted. If null, returns null.</param>
		/// <returns>
		///  The converted value as an object, or null if:
		///  <list type="bullet">
		///   <li>The input value is null</li>
		///   <li>The conversion fails</li>
		///  </list>
		/// </returns>
		/// <remarks>
		///  This method handles conversion in the following priority:
		///  <list type="number">
		///   <li>Returns null if the input value is null</li>
		///   <li>Uses predefined type converters from the TypeConverters dictionary for common types</li>
		///   <li>Directly converts to string if the target type is string</li>
		///   <li>Falls back to Convert.ChangeType with InvariantCulture for other types</li>
		///  </list>
		///  If the conversion fails at any point, the method returns null.
		/// </remarks>
		private static object Cast(Type type, object value) {
			if (value == null) {
				return null;
			}
			Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;
			// Check if we have a converter for this type
			var converter = GetTypeConverters(underlyingType);
			if(converter != null) {
				return converter(value);
			}
			
			// Special case for string
			if (type == typeof(string)) {
				return value.ToString();
			}

			// For types we don't have specific converters for
			try {
				return Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
			}
			catch {
				return null;
			}
		}

		/// <summary>
		///  Determines whether a property represents a collection type.
		/// </summary>
		/// <param name="prop">The property information to check.</param>
		/// <returns>
		///  <c>true</c> if the property represents a collection type; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		///  A property is considered a collection if it satisfies any of the following conditions:
		///  <list type="bullet">
		///   <item>
		///    <description>It is an array type</description>
		///   </item>
		///   <item>
		///    <description>
		///     It is a generic type implementing one of the following interfaces:
		///     <list type="bullet">
		///      <item>
		///       <description>
		///        <see cref="List{T}" />
		///       </description>
		///      </item>
		///      <item>
		///       <description>
		///        <see cref="IEnumerable{T}" />
		///       </description>
		///      </item>
		///      <item>
		///       <description>
		///        <see cref="ICollection{T}" />
		///       </description>
		///      </item>
		///      <item>
		///       <description>
		///        <see cref="IReadOnlyCollection{T}" />
		///       </description>
		///      </item>
		///     </list>
		///    </description>
		///   </item>
		///   <item>
		///    <description>It implements <see cref="IEnumerable" /> interface (except for <see cref="string" /> type)</description>
		///   </item>
		///  </list>
		/// </remarks>

		
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "S1067:Methods should not have too many statements", 
			Justification = "Reviewed. - Could not come up with a better way yet")]
		private static bool IsCollection(PropertyInfo prop) {
			Type type = prop.PropertyType;
			return type.IsArray ||
				(type.IsGenericType &&
					(type.GetGenericTypeDefinition() == typeof(List<>) ||
						type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
						type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
						type.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>))) ||
				(typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string));
		}
		private static bool IsOutputParameter(ProcessParameterAttribute ppa) {
			return ppa != null &&
				(ppa.Direction == ProcessParameterDirection.Output ||
					ppa.Direction == ProcessParameterDirection.Bidirectional);
		}

		/// <summary>
		///  Parses a JSON array into a strongly-typed collection.
		/// </summary>
		/// <remarks>
		///  This method handles the conversion of JSON array data into a strongly-typed collection by:
		///  <list type="bullet">
		///   <item>Creating a generic List of the appropriate collection item type</item>
		///   <item>Iterating through each item in the JSON array</item>
		///   <item>Creating instances of the collection item type</item>
		///   <item>Populating each instance's properties based on matching JSON property names</item>
		///   <item>Adding the populated instances to the list</item>
		///  </list>
		/// </remarks>
		/// <param name="prop">The property information of the collection to be populated</param>
		/// <param name="value">The JSON array containing the collection data</param>
		/// <returns>
		///  A populated instance of a generic List containing objects of the collection's item type
		///  with properties set from the JSON data
		/// </returns>
		private static object ParseCollection(PropertyInfo prop, JArray value) {
			Type collectionItemType = prop.PropertyType.UnderlyingSystemType.GetCollectionItemType();
			Type listType = typeof(List<>).MakeGenericType(collectionItemType);
			object list = Activator.CreateInstance(listType);
			foreach (JToken jValue in value) {
				// jValue is json of Collection item
				object listItemInstance = Activator.CreateInstance(collectionItemType);
				int count = jValue.Count();

				for (int i = 0; i < count; i++) {
					JProperty jProperty = jValue.ElementAt(i) as JProperty;
					if (jProperty == null) {
						continue;
					}
					string jn = jProperty.Name;
					JToken jv = jProperty.Value;
					PropertyInfo instanceProp = collectionItemType.GetProperty(jn,
						BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
					if (instanceProp == null) {
						continue;
					}
					object c = Cast(instanceProp.PropertyType, jv);
					instanceProp.SetValue(listItemInstance, c);
				}
				((IList)list).Add(listItemInstance);
			}
			return list;
		}

		#endregion

		#region Methods: Public

		/// <summary>
		///  Parses a JSON response from a Creatio process execution into a strongly-typed model.
		/// </summary>
		/// <typeparam name="T">
		///  The type of model to parse the response into. Must inherit from BaseBpModel and have a
		///  parameterless constructor.
		/// </typeparam>
		/// <param name="json">The JSON string containing the RunProcessResponse from Creatio.</param>
		/// <returns>
		///  A <see cref="RunProcessResponseWrapper{T}" /> containing both the original response and the parsed model,
		///  or null if the process execution was not successful.
		/// </returns>
		/// <remarks>
		///  The method works by:
		///  <list type="number">
		///   <li>Deserializing the JSON into a RunProcessResponse object</li>
		///   <li>Creating a new instance of the specified model type</li>
		///   <li>Identifying properties in the model marked with ProcessParameterAttribute</li>
		///   <li>Mapping output parameters from the response to the corresponding model properties</li>
		///   <li>Handling special cases for collections by parsing them appropriately</li>
		///  </list>
		///  This method is typically used by data providers to transform raw service responses into domain models.
		/// </remarks>
		
		// ReSharper disable once CognitiveComplexity
		public static RunProcessResponseWrapper<T> Parse<T>(string json) where T : BaseBpModel, new() {
			try {
				RunProcessResponse response = JsonConvert.DeserializeObject<RunProcessResponse>(json);
				if (!response.Success) {
					return new RunProcessResponseWrapper<T>(response);
				}
				T model = new T();
				model.GetType().GetProperties().ToList().ForEach(prop => {
					ProcessParameterAttribute ppa = prop.GetCustomAttribute<ProcessParameterAttribute>(true);
					if (IsOutputParameter(ppa) && response.ResultParameterValues != null &&
						response.ResultParameterValues.ContainsKey(ppa.Name)) {
						bool isValue = response.ResultParameterValues.TryGetValue(ppa.Name, out object value);
						if (isValue && value != null) {
							if (IsCollection(prop)) {
								object list = ParseCollection(prop, value as JArray);
								prop.SetValue(model, list);
							}
							else {
								prop.SetValue(model, Cast(prop.PropertyType, value));
							}
						}
					}
				});
				return new RunProcessResponseWrapper<T>(response) {
					ResultModel = model
				};
			}
			catch (Exception e) {
				return new RunProcessResponseWrapper<T>(new RunProcessResponse {
					Success = false,
					ErrorInfo = new ErrorInfo {
						Message = e.Message,
						StackTrace = e.StackTrace
					}
				});
			}
		}

		#endregion

	}
}
