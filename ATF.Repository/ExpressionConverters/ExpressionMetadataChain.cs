namespace ATF.Repository.ExpressionConverters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Terrasoft.Common;

	internal class ExpressionMetadataChain
	{

		public Type LastValueType { get; set; }
		public List<ExpressionMetadataChainItem> Items { get; set; }

		internal ExpressionMetadataChain() {
			Items = new List<ExpressionMetadataChainItem>();
		}

		internal bool IsEmpty() {
			return Items.IsEmpty();
		}

		internal Type GetModelType() {
			return Items.Any() ? Items.First().InputDtoType.Type : LastValueType;
		}

		internal Type GetOutputType() {
			return Items.Any() ? Items.Last().OutputDtoType.Type : LastValueType;
		}

		public void ValidateChainItems(List<string> availableMethods) {
			Items.ForEach(x=>ValidateChainItem(x, availableMethods));
		}

		private void ValidateChainItem(ExpressionMetadataChainItem expressionMetadataChainItem, List<string> availableMethods) {
			var methodName = expressionMetadataChainItem.ExpressionMetadata?.MethodName;
			if (!string.IsNullOrEmpty(methodName) && !availableMethods.Contains(methodName)) {
				throw new FormatException($"Method {methodName} is not available for expression");
			}
		}
	}
}
