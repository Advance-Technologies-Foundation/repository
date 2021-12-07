using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ATF.Repository.ExpressionConverters;
using Terrasoft.Common;
using Terrasoft.Core.Entities;

namespace ATF.Repository.ExpressionConverters
{
	internal class FilterConverter
	{
		public static ExpressionMetadata Convert(Expression expression, ExpressionModelMetadata modelMetadata) {
			var rawExpressionMetadata = ExpressionMetadataRawParser.Parse(expression, modelMetadata);
			var normalizedExpressionMetadata = RawExpressionMetadataNormalizer.Normalize(rawExpressionMetadata);
			return RawToExpressionMetadataConverter.Convert(normalizedExpressionMetadata, modelMetadata);

		}
	}
}
