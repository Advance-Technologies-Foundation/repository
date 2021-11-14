namespace ATF.Repository.ExpressionConverters
{
	using System;

	internal class ExpressionModelMetadata {
		internal Type Type { get; set; }
		internal string Name { get; set; }

		internal string ColumnPath { get; set; }

		internal ExpressionModelMetadata Parent { get; set; }
	}
}
