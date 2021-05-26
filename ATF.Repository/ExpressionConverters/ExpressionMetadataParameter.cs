namespace ATF.Repository.ExpressionConverters
{
	using System;

	internal class ExpressionMetadataParameter {
		internal Type Type { get; set; }
		internal object Value { get; set; }
		internal string ColumnPath { get; set; }
	}
}
