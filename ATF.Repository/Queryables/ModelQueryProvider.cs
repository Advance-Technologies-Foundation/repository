namespace ATF.Repository.Queryables
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using ATF.Repository.ExpressionAppliers;
	using ATF.Repository.ExpressionConverters;
	using Terrasoft.Common;
	using ATF.Repository.Providers;

	internal class ModelQueryProvider: ExpressionVisitor, IQueryProvider
	{
		private readonly IDataProvider _dataProvider;
		private readonly AppDataContext _appDataContext;
		private readonly Type _elementType;

		internal ModelQueryProvider(IDataProvider dataProvider, AppDataContext appDataContext, Type elementType) {
			_dataProvider = dataProvider;
			_appDataContext = appDataContext;
			_elementType = elementType;
		}

		public IEnumerable<T> ExecuteEnumerable<T>(Expression expression) {
			var chain = ExpressionToMetadataConverter.Convert(expression, _elementType);
			var dataCollection = LoadDataCollection(chain);

			// If there's a Select projection that was applied to query,
			// we need to convert Dictionary results directly to anonymous types
			if (HasSelectProjection(chain)) {
				return LoadProjectedCollection<T>(dataCollection, chain);
			}

			// Otherwise, load full models and apply any client-side projections
			var models = LoadModelCollection(dataCollection, chain);
			return ApplyCollectionProjector<T>(models.ToList<object>(), chain);
		}

		private IEnumerable<T> ApplyCollectionProjector<T>(IReadOnlyCollection<object> sourceItems, ExpressionMetadataChain chain) {
			var notAppliedChainItems = chain.Items.Where(x => !x.IsAppliedToQuery).ToList();
			if (notAppliedChainItems.Any()) {
				var method = RepositoryReflectionUtilities.GetGenericMethod(this.GetType(), "ApplyTypedCollectionProjector", typeof(T),
					notAppliedChainItems.First().InputDtoType.Type);
				return (IEnumerable<T>)method.Invoke(this, new object[] {sourceItems, chain});
			}
			return sourceItems.Select(x => (T) x).AsEnumerable();
		}

		private IEnumerable<T> ApplyTypedCollectionProjector<T, TItem>(IEnumerable<object> rawSourceItems, ExpressionMetadataChain chain) {
			var sourceItems = rawSourceItems.Select(x => (TItem) x).ToList();
			Expression sourceExpression = Expression.Constant(sourceItems.AsQueryable());
			chain.Items.Where(x=>!x.IsAppliedToQuery).ForEach(x => {
				sourceExpression = Expression.Call(null, x.Expression.Method, sourceExpression,
					x.Expression.Arguments.Skip(1).First());
			});

			return (IEnumerable<T>)Expression.Lambda(sourceExpression).Compile().DynamicInvoke();
		}

		public IEnumerable ExecuteEnumerable(Type type, Expression expression) {
			var method = RepositoryReflectionUtilities.GetGenericMethod(this.GetType(), "ExecuteEnumerable", type);
			return (IEnumerable)method.Invoke(this, new object[] {expression});
		}

		public IQueryable CreateQuery(Expression expression) {
			throw new System.NotSupportedException();
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression) {
			expression.CheckArgumentNull(nameof(expression));
			return new ModelQuery<TElement>(_dataProvider, this, expression);
		}

		public object Execute(Expression expression) {
			throw new System.NotSupportedException();
		}

		public TResult Execute<TResult>(Expression expression) {
			return ExecuteScalar<TResult>(expression);
		}

		private List<BaseModel> LoadModelCollection(List<Dictionary<string, object>> dataCollection, ExpressionMetadataChain chain) {
			var method = RepositoryReflectionUtilities.GetGenericMethod(GetType(),
				"LoadTypedModelCollection", chain.GetModelType());
			var models = (List<BaseModel>)method?.Invoke(this, new object[] {dataCollection});
			return models;
		}

		private List<BaseModel> LoadTypedModelCollection<T>(List<Dictionary<string, object>> dataCollection) where T: BaseModel, new() {
			var models = _appDataContext.GetModelsByDataCollection<T>(dataCollection);
			return models.Select(x=>(BaseModel)x).ToList();
		}

		private List<Dictionary<string, object>> LoadDataCollection(ExpressionMetadataChain chain) {
			var selectQuery = ModelQueryBuilder.BuildSelectQuery(chain);
			var response = _dataProvider.GetItems(selectQuery);
			return response != null && response.Success
				? response.Items
				: new List<Dictionary<string, object>>();
		}

		private T ExecuteScalar<T>(Expression expression) {
			var chain = ExpressionToMetadataConverter.Convert(expression, _elementType);
			var dataCollection = LoadDataCollection(chain);
			if (RepositoryExpressionUtilities.IsAggregationMethodExpression(expression)) {
				return GetAggregationValue<T>(expression, dataCollection);
			}
			if (RepositoryExpressionUtilities.IsAnyMethodExpression(expression)) {
				return GetAnyValue<T>(dataCollection);
			}

			if (HasSelectProjection(chain)) {
				var projected = LoadProjectedCollection<T>(dataCollection, chain);
				return projected.FirstOrDefault();
			}

			var models = LoadModelCollection(dataCollection, chain);
			return ApplyScalarProjector<T>(models.ToList<object>(), chain);
		}

		private static T GetAnyValue<T>(IReadOnlyCollection<Dictionary<string, object>>
			dataCollection) {
			var columnName = RepositoryExpressionUtilities.GetAnyColumnName();
			var count = GetAggregationValue<int>(columnName, dataCollection);
			var value = count > 0;
			if (value is T typedValue) {
				return typedValue;
			}

			return default(T);
		}

		private static T GetAggregationValue<T>(Expression expression, IReadOnlyCollection<Dictionary<string, object>> dataCollection) {
			var methodName = RepositoryExpressionUtilities.GetMethodName(expression);
			var columnName = RepositoryExpressionUtilities.GetAggregationColumnName(methodName);
			return GetAggregationValue<T>(columnName, dataCollection);
		}

		private static T GetAggregationValue<T>(string columnName, IReadOnlyCollection<Dictionary<string, object>> dataCollection) {
			if (!dataCollection.Any()) {
				return default(T);
			}

			var data = dataCollection.First();
			if (!data.ContainsKey(columnName)) {
				return default(T);
			}

			var value = data[columnName];
			if (value == null) {
				return default(T);
			}

			if (value is T typedValue) {
				return typedValue;
			}
			var converter = TypeDescriptor.GetConverter(data[columnName].GetType());
			if (!converter.CanConvertTo(typeof(T))) {
				return default(T);
			}

			return (T)converter.ConvertTo(value, typeof(T));
		}

		private T ApplyScalarProjector<T>(List<object> sourceItems, ExpressionMetadataChain chain) {
			var notAppliedChainItems = chain.Items.Where(x => !x.IsAppliedToQuery).ToList();
			if (!notAppliedChainItems.Any()) {
				return sourceItems.Any() ? (T) sourceItems.First() : default(T);
			}
			var method = RepositoryReflectionUtilities.GetGenericMethod(this.GetType(), "ApplyTypedScalarProjector", typeof(T),
				notAppliedChainItems.First().InputDtoType.Type);
			return (T)method.Invoke(this, new object[] {sourceItems, chain});

		}

		private T ApplyTypedScalarProjector<T, TItem>(List<object> rawSourceItems, ExpressionMetadataChain chain) {
			var sourceItems = rawSourceItems.Select(x => (TItem) x).ToList();
			Expression sourceExpression = Expression.Constant(sourceItems.AsQueryable());
			chain.Items.Where(x=>!x.IsAppliedToQuery).ForEach(x => {
				if (x.Expression.Arguments.Count == 1) {
					sourceExpression = Expression.Call(null, x.Expression.Method, sourceExpression);
				} else if (x.Expression.Arguments.Count == 2) {
					sourceExpression = Expression.Call(null, x.Expression.Method, sourceExpression,
						x.Expression.Arguments.Skip(1).First());
				} else {
					throw new System.NotSupportedException();
				}
			});

			return (T)Expression.Lambda(sourceExpression).Compile().DynamicInvoke();
		}

		private bool HasSelectProjection(ExpressionMetadataChain chain) {
			return chain.Items.Any(x =>
				x.IsAppliedToQuery &&
				(x.Expression.Method.Name == ConvertableExpressionMethod.Select ||
				 x.Expression.Method.Name == ConvertableExpressionMethod.GroupBy));
		}

		/// <summary>
		/// Load projected collection directly from Dictionary results (for Select/GroupBy)
		/// This bypasses model creation and converts Dictionary -> anonymous type directly
		/// </summary>
		private IEnumerable<T> LoadProjectedCollection<T>(List<Dictionary<string, object>> dataCollection, ExpressionMetadataChain chain) {
			// Get the Select or GroupBy chain item to extract the selector expression
			var projectionItem = chain.Items.FirstOrDefault(x =>
				x.IsAppliedToQuery &&
				(x.Expression.Method.Name == ConvertableExpressionMethod.Select ||
				 x.Expression.Method.Name == ConvertableExpressionMethod.GroupBy));

			if (projectionItem == null) {
				throw new InvalidOperationException("Select or GroupBy projection not found in chain");
			}

			// Get the selector lambda expression
			// For Select: x => new { x.Name, x.Email } (second argument)
			// For GroupBy: (groupBy, items) => new { ... } (third argument - resultSelector)
			var selectorExpression = projectionItem.Expression.Method.Name == ConvertableExpressionMethod.GroupBy
				? projectionItem.Expression.Arguments[2]  // GroupBy resultSelector
				: projectionItem.Expression.Arguments[1]; // Select selector

			// Get column mappings from the projection metadata
			var projectionMetadata = projectionItem.ExpressionMetadata;

			// Compile a function that takes Dictionary<string, object> and returns T
			var converter = BuildDictionaryToAnonymousTypeConverter<T>(selectorExpression, projectionMetadata);

			// Apply the converter to each dictionary
			return dataCollection.Select(dict => converter(dict)).ToList();
		}

		/// <summary>
		/// Build a converter function for single property selection: Dictionary<string, object> -> T
		/// Handles cases like Select(x => x.Name)
		/// </summary>
		private Func<Dictionary<string, object>, T> BuildSinglePropertyConverter<T>(ExpressionMetadata selectMetadata, MemberExpression memberExpr) {
			// For single property, we expect exactly one item in metadata
			if (selectMetadata?.Items == null || selectMetadata.Items.Count != 1) {
				throw new InvalidOperationException($"Single property select must have exactly one metadata item, but got {selectMetadata?.Items?.Count ?? 0}");
			}

			var metadataItem = selectMetadata.Items[0];
			var dictionaryKey = metadataItem.Code ?? metadataItem.Parameter?.ColumnPath;

			if (string.IsNullOrEmpty(dictionaryKey)) {
				throw new InvalidOperationException("Dictionary key not found for single property Select");
			}

			// Return a function that extracts the value from the dictionary
			return dict => {
				if (dict.TryGetValue(dictionaryKey, out var value)) {
					return (T)ConvertValue(value, typeof(T));
				}
				return default(T);
			};
		}

		/// <summary>
		/// Build a converter function: Dictionary<string, object> -> T (anonymous type)
		/// Uses constructor-based approach for simplicity and reliability
		/// </summary>
		private Func<Dictionary<string, object>, T> BuildDictionaryToAnonymousTypeConverter<T>(Expression selectorExpression, ExpressionMetadata selectMetadata) {
			// selectorExpression is a LambdaExpression like: (Contact x) => new { x.Name, RecordType = "Contact" }
			// We need to extract the constructor and build arguments from Dictionary values and constants

			// Handle UnaryExpression with Quote node type (wrapped lambda)
			if (selectorExpression.NodeType == ExpressionType.Quote) {
				selectorExpression = ((UnaryExpression)selectorExpression).Operand;
			}

			var lambda = selectorExpression as LambdaExpression;
			if (lambda == null) {
				throw new InvalidOperationException($"Selector expression must be a lambda, but got {selectorExpression.GetType().Name} with NodeType {selectorExpression.NodeType}");
			}

			// Handle single property selection (e.g., Select(x => x.Name))
			var memberExpr = lambda.Body as MemberExpression;
			if (memberExpr != null) {
				return BuildSinglePropertyConverter<T>(selectMetadata, memberExpr);
			}

			// Check if it's a NewExpression for anonymous type
			var newExpr = lambda.Body as NewExpression;
			if (newExpr != null) {
				// It's an anonymous type - process it normally (code continues below)
			} else {
				// Not a NewExpression - must be single property selection
				// Handle Terrasoft PropertyExpression (custom expression type from Terrasoft.Core)
				// This occurs when using MemoryDataProviderMock which uses Terrasoft expression trees
				return BuildSinglePropertyConverter<T>(selectMetadata, null);
			}

			// Get the constructor
			var constructor = newExpr.Constructor;
			if (constructor == null) {
				throw new InvalidOperationException("Anonymous type constructor not found");
			}

			// Build value sources: either dictionary key or constant value
			var parameterTypes = constructor.GetParameters();
			var valueSources = new List<ValueSource>();

			if (selectMetadata?.Items == null || selectMetadata.Items.Count == 0) {
				throw new InvalidOperationException("Select metadata items are null or empty");
			}

			// Validate counts match
			if (selectMetadata.Items.Count != parameterTypes.Length) {
				throw new InvalidOperationException(
					$"Metadata count ({selectMetadata.Items.Count}) does not match constructor parameter count ({parameterTypes.Length}). " +
					$"Constructor: {constructor.DeclaringType?.Name}");
			}

			for (int i = 0; i < selectMetadata.Items.Count; i++) {
				var metadataItem = selectMetadata.Items[i];

				if (metadataItem.NodeType == ExpressionMetadataNodeType.Constant) {
					// Constant value - use directly
					valueSources.Add(new ValueSource {
						IsConstant = true,
						ConstantValue = metadataItem.Parameter?.Value
					});
				} else {
					// Column from dictionary
					var dictionaryKey = metadataItem.Code ?? metadataItem.Parameter?.ColumnPath;
					if (string.IsNullOrEmpty(dictionaryKey)) {
						throw new InvalidOperationException($"Dictionary key not found for Select item at index {i}");
					}

					valueSources.Add(new ValueSource {
						IsConstant = false,
						DictionaryKey = dictionaryKey
					});
				}
			}

			// Return a function that builds constructor arguments and calls the constructor
			return dict => {
				var constructorArgs = new object[valueSources.Count];

				for (int i = 0; i < valueSources.Count; i++) {
					var source = valueSources[i];
					var targetType = parameterTypes[i].ParameterType;

					if (source.IsConstant) {
						// Use constant value directly
						constructorArgs[i] = ConvertValue(source.ConstantValue, targetType);
					} else {
						// Get value from dictionary
						if (!dict.ContainsKey(source.DictionaryKey)) {
							throw new InvalidOperationException($"Dictionary does not contain key '{source.DictionaryKey}'. Available keys: {string.Join(", ", dict.Keys)}");
						}

						var value = dict[source.DictionaryKey];
						constructorArgs[i] = ConvertValue(value, targetType);
					}
				}

				// Call the constructor with the arguments
				return (T)constructor.Invoke(constructorArgs);
			};
		}

		/// <summary>
		/// Describes where to get a value for a constructor parameter
		/// </summary>
		private class ValueSource {
			public bool IsConstant { get; set; }
			public object ConstantValue { get; set; }
			public string DictionaryKey { get; set; }
		}

		/// <summary>
		/// Convert a value from Dictionary to the target type
		/// </summary>
		private object ConvertValue(object value, Type targetType) {
			if (value == null) {
				return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
			}

			var valueType = value.GetType();

			// If types match, return as-is
			if (targetType.IsAssignableFrom(valueType)) {
				return value;
			}

			// Handle BaseModel types (lookup properties in GroupBy results)
			if (typeof(BaseModel).IsAssignableFrom(targetType)) {
				if (value is Dictionary<string, object> lookupDict) {
					// Create BaseModel instance from nested dictionary data (full object)
					return CreateBaseModelFromDictionary(lookupDict, targetType);
				} else if (value is Guid guidValue) {
					// GroupBy returns just the Guid for lookup columns, not the full object
					// Create a minimal BaseModel instance with just the Id set
					var instance = Activator.CreateInstance(targetType) as BaseModel;
					if (instance != null) {
						instance.Id = guidValue;
					}
					return instance;
				}
			}

			// Handle nullable types
			if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
				var underlyingType = Nullable.GetUnderlyingType(targetType);
				var convertedValue = ConvertValue(value, underlyingType);
				return convertedValue;
			}

			// Try type conversion
			try {
				return Convert.ChangeType(value, targetType);
			} catch {
				// If conversion fails, try TypeDescriptor
				var converter = TypeDescriptor.GetConverter(targetType);
				if (converter.CanConvertFrom(valueType)) {
					return converter.ConvertFrom(value);
				}

				throw new InvalidOperationException($"Cannot convert value of type {valueType.Name} to {targetType.Name}");
			}
		}

		/// <summary>
		/// Create a BaseModel instance from dictionary data (for GroupBy lookup results)
		/// </summary>
		private object CreateBaseModelFromDictionary(Dictionary<string, object> dict, Type modelType) {
			// For GroupBy results with lookup columns, we may not have Id
			// Try to get the model through AppDataContext if Id exists, otherwise create a simple instance
			if (dict.ContainsKey("Id") && dict["Id"] is Guid id && id != Guid.Empty) {
				// Use AppDataContext to create tracked model instance
				var method = typeof(AppDataContext).GetMethod("GetModelsByDataCollection",
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				if (method != null) {
					var genericMethod = method.MakeGenericMethod(modelType);
					var dataCollection = new List<Dictionary<string, object>> { dict };
					var models = (List<BaseModel>)genericMethod.Invoke(_appDataContext, new object[] { dataCollection });
					return models?.FirstOrDefault();
				}
			}

			// If no Id or AppDataContext method failed, create a simple untracked instance
			// and populate properties from dictionary
			var instance = Activator.CreateInstance(modelType) as BaseModel;
			if (instance != null) {
				var properties = modelType.GetProperties();
				foreach (var prop in properties) {
					if (dict.ContainsKey(prop.Name) && prop.CanWrite) {
						try {
							var propValue = dict[prop.Name];
							if (propValue != null && prop.PropertyType.IsAssignableFrom(propValue.GetType())) {
								prop.SetValue(instance, propValue);
							} else if (propValue != null) {
								var converted = ConvertValue(propValue, prop.PropertyType);
								prop.SetValue(instance, converted);
							}
						} catch {
							// Skip properties that can't be set
						}
					}
				}
			}

			return instance;
		}

	}
}
