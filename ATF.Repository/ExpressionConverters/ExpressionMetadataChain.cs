using System;
using System.Collections.Generic;
using System.Linq;
using Terrasoft.Common;

namespace ATF.Repository.ExpressionConverters
{
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
	}
}
