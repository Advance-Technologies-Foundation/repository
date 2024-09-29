namespace ATF.Repository.UnitTests.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Terrasoft.Core.Entities;
	using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;

	public static class QueryComparison
	{
		public static bool AreSelectQueryEqual(ISelectQuery expected, ISelectQuery actual) {
			var actualJson = JsonConvert.SerializeObject(actual);
			var areMainEqual = AreMainSelectQueryParametersEqual(expected, actual);
			var areColumnsEqual = AreSelectQueryColumnsEqual(expected, actual);
			var areFiltersEqual = AreSelectQueryFilterGroupEqual(expected.Filters, actual.Filters);
			var expectedFilter = JsonConvert.SerializeObject(expected.Filters);
			var actualFilter = JsonConvert.SerializeObject(actual.Filters);
			return areMainEqual && areColumnsEqual && areFiltersEqual;
		}

		private static bool AreSelectQueryFilterGroupEqual(IFilter expected, IFilter actual) {
			var areEqual = expected.FilterType == actual.FilterType &&
				expected.LogicalOperation == actual.LogicalOperation &&
				AreSelectQueryFilterItemsEqual(expected.Items, actual.Items);
			return areEqual;
		}

		private static bool AreSelectQueryFilterItemsEqual(Dictionary<string, IFilter> expected,
			Dictionary<string, IFilter> actual) {
			var foundedKeys = new List<string>();
			return expected.Count == actual.Count && expected.All(expectedItem => {
				var founded = actual.FirstOrDefault(actualItem =>
					!foundedKeys.Contains(actualItem.Key) &&
					AreSelectQueryFilterEqual(expectedItem.Value, actualItem.Value));
				if (founded.Value == null)
					return false;
				foundedKeys.Add(founded.Key);
				return true;
			});
		}

		private static bool AreSelectQueryFilterEqual(IFilter expected, IFilter actual) {
			return expected.FilterType == actual.FilterType && expected.FilterType == FilterType.FilterGroup
				? AreSelectQueryFilterGroupEqual(expected, actual)
				: AreSelectQueryComparisonFilterEqual(expected, actual);
		}

		private static bool AreSelectQueryComparisonFilterEqual(IFilter expected, IFilter actual) {
			var areEqual = expected.ComparisonType == actual.ComparisonType &&
				expected.IsEnabled == actual.IsEnabled &&
				AreSelectQueryExpressionEqual(expected.LeftExpression, actual.LeftExpression) &&
				AreSelectQueryExpressionEqual(expected.RightExpression, actual.RightExpression) &&
				AreSelectQueryExpressionEqual(expected.RightExpressions, actual.RightExpressions);
			return areEqual;
		}

		private static bool AreSelectQueryExpressionEqual(IBaseExpression expected, IBaseExpression actual) {
			if (expected == null && actual == null) {
				return true;
			}

			if (expected == null || actual == null || expected.ExpressionType != actual.ExpressionType) {
				return false;
			}

			switch (expected.ExpressionType) {
				case EntitySchemaQueryExpressionType.SchemaColumn:
					return expected.ColumnPath == actual.ColumnPath;
				case EntitySchemaQueryExpressionType.Parameter:
					return (expected.Parameter.Value == null && actual.Parameter.Value == null) ||
						(expected.Parameter.Value != null &&
							expected.Parameter.DataValueType == actual.Parameter.DataValueType &&
							((expected.Parameter.Value == null && (actual.Parameter.Value == null) ||
								expected.Parameter.Value.Equals(actual.Parameter.Value))));
				case EntitySchemaQueryExpressionType.SubQuery:
					return CompareSubQuery(expected, actual);
				case EntitySchemaQueryExpressionType.Function:
					return CompareFunctionColumn(expected, actual);
				default:
					throw new NotImplementedException();
			}
		}

		private static bool CompareFunctionColumn(IBaseExpression expected, IBaseExpression actual) {
			return expected.ExpressionType == actual.ExpressionType &&
				expected.FunctionType == actual.FunctionType &&
				expected.DatePartType == actual.DatePartType &&
				CompareFunctionArgument(expected.FunctionArgument, actual.FunctionArgument);
		}

		private static bool CompareFunctionArgument(IBaseExpression expected, IBaseExpression actual) {
			return (expected == null && actual == null) || (expected != null && actual != null &&
				expected.ExpressionType == actual.ExpressionType &&
				expected.ColumnPath == actual.ColumnPath);
		}

		private static bool CompareSubQuery(IBaseExpression expected, IBaseExpression actual) {
			var isParametersEqual = expected.ExpressionType == actual.ExpressionType &&
				expected.AggregationType == actual.AggregationType &&
				expected.FunctionType == actual.FunctionType &&
				expected.ColumnPath == actual.ColumnPath;
			var isSubFiltersEqual = AreSelectQueryFilterGroupEqual(expected.SubFilters, actual.SubFilters);
			var expectedFilter = JsonConvert.SerializeObject(expected.SubFilters);
			var actualFilter = JsonConvert.SerializeObject(actual.SubFilters);

			return isParametersEqual && isSubFiltersEqual;
		}

		private static bool AreSelectQueryExpressionEqual(IBaseExpression[] expected, IBaseExpression[] actual) {
			if (expected == null && actual == null) {
				return true;
			}

			var foundedExpressions = new List<IBaseExpression>();
			return expected.Length == actual.Length && expected.All(expectedItem => {
				var founded = actual.FirstOrDefault(actualItem =>
					!foundedExpressions.Contains(actualItem) &&
					AreSelectQueryExpressionEqual(expectedItem, actualItem));
				if (founded != null) {
					foundedExpressions.Add(founded);
					return true;
				}

				return false;
			});
		}

		private static bool AreSelectQueryColumnsEqual(ISelectQuery expected, ISelectQuery actual) {
			var areEqual = expected.Columns.Items.Count == actual.Columns.Items.Count &&
				expected.Columns.Items.All(item =>
					actual.Columns.Items.ContainsKey(item.Key) &&
					AreSelectQueryColumnEqual(item.Value, actual.Columns.Items[item.Key]));
			return areEqual;
		}

		private static bool AreSelectQueryColumnEqual(ISelectQueryColumn expected, ISelectQueryColumn actual) {
			var areEqual = expected.Expression.ColumnPath == actual.Expression.ColumnPath &&
				expected.Expression.ExpressionType == actual.Expression.ExpressionType &&
				expected.OrderDirection == actual.OrderDirection &&
				expected.OrderPosition == actual.OrderPosition;
			return areEqual;
		}

		private static bool AreMainSelectQueryParametersEqual(ISelectQuery expected, ISelectQuery actual) {
			return expected.RootSchemaName == actual.RootSchemaName &&
				expected.AllColumns == actual.AllColumns &&
				expected.IsPageable == actual.IsPageable &&
				expected.RowCount == actual.RowCount &&
				expected.IsDistinct == actual.IsDistinct;
		}
	}
}
