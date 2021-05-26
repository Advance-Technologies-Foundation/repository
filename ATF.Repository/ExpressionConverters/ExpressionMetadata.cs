
namespace ATF.Repository.ExpressionConverters
{
	using System.Collections.Generic;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;

	internal class ExpressionMetadata {
		internal ExpressionMetadataNodeType NodeType { get; set; }
		internal ExpressionMetadata LeftExpression { get; set; }
		internal List<ExpressionMetadata> RightExpressions { get; set; }
		internal FilterComparisonType ComparisonType { get; set; }
		internal LogicalOperationStrict LogicalOperation { get; set; }
		internal List<ExpressionMetadata> Items { get; set; }

		internal ExpressionMetadataParameter Parameter { get; set; }

		internal ExpressionModelMetadata ModelMetadata { get; set; }

		internal bool IsNot { get; set; }

		internal ExpressionMetadata() {
			RightExpressions = new List<ExpressionMetadata>();
			LogicalOperation = LogicalOperationStrict.And;
			Items = new List<ExpressionMetadata>();
		}
	}
}
