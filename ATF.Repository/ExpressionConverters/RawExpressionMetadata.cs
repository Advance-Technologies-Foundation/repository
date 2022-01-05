namespace ATF.Repository.ExpressionConverters
{
	using System.Collections.Generic;
	using System.Linq.Expressions;

	internal class RawExpressionMetadata
	{
		public ExpressionType Type { get; set; }
		public RawExpressionMetadata Left { get; set; }
		public RawExpressionMetadata Right { get; set; }
		public ExpressionMetadataParameter Parameter { get; set; }
		public List<RawExpressionMetadata> Items { get;  }

		public AvailableFieldMethod FieldMethod { get; set; }

		public AvailableColumnMethod ColumnMethod { get; set; }

		public RawDetailExpressionMetadata RawDetailExpressionMetadata { get; set; }

		public bool IsNot { get; set; }

		internal RawExpressionMetadata() {
			Items = new List<RawExpressionMetadata>();
		}
	}

	internal class RawDetailExpressionMetadata
	{
		public string CorePath { get; set; }
		public MemberExpression DetailProperty { get; set; }
		public MethodCallExpression FullExpression { get; set; }
	}
}
