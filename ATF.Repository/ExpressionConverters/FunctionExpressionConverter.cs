namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;

	internal abstract class FunctionExpressionConverter : ExpressionConverter
	{
		private class FunctionConverterInfo
		{
			public Type DeclaringType { get; set; }
			public string Name { get; set; }
			public Type Converter { get; set; }
		}

		private static readonly List<FunctionConverterInfo> FunctionConverterInfos = new List<FunctionConverterInfo>() {
			new FunctionConverterInfo() {
				DeclaringType = typeof(string),
				Name = "StartsWith",
				Converter = typeof(StartsWithFunctionExpressionConverter)
			},
			new FunctionConverterInfo() {
				DeclaringType = typeof(string),
				Name = "EndsWith",
				Converter = typeof(EndsWithFunctionExpressionConverter)
			},
			new FunctionConverterInfo() {
				DeclaringType = typeof(string),
				Name = "Contains",
				Converter = typeof(StringContainsFunctionExpressionConverter)
			}
		};

		protected readonly MethodCallExpression Node;

		internal FunctionExpressionConverter(MethodCallExpression expression) {
			Node = expression;
		}

		internal static ExpressionMetadata Convert(MethodCallExpression expression, ExpressionModelMetadata modelMetadata) {
			MethodInfo methodInfo = expression.Method;
			var converterInfo = FunctionConverterInfos.FirstOrDefault(item =>
				item.DeclaringType == methodInfo.DeclaringType && item.Name == methodInfo.Name);
			if (converterInfo == null) {
				throw new NotImplementedException();
			}
			var converter = GetConverter(converterInfo, expression, modelMetadata);
			return converter?.ConvertNode();
		}

		private static ExpressionConverter GetConverter(FunctionConverterInfo converterInfo, Expression expression, ExpressionModelMetadata modelMetadata) {
			var instance = Activator.CreateInstance(converterInfo.Converter, expression) as ExpressionConverter;
			if (instance != null)
				instance.modelMetadata = modelMetadata;
			return instance;
		}
	}
}
