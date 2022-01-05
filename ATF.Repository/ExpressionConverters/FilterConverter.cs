namespace ATF.Repository.ExpressionConverters
{
	using System.Linq.Expressions;

	internal class FilterConverter
	{
		public static ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			var rawExpressionMetadata = ExpressionMetadataRawParser.Parse(expression, modelMetadata);
			var normalizedExpressionMetadata = RawExpressionMetadataNormalizer.Normalize(rawExpressionMetadata);
			return RawToExpressionMetadataConverter.Convert(normalizedExpressionMetadata, modelMetadata);

		}
	}
}
