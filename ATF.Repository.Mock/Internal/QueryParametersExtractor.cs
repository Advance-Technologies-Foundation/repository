using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Terrasoft.Common;
using Terrasoft.Core.Entities;
using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;
using DataValueType = Terrasoft.Nui.ServiceModel.DataContract.DataValueType;

namespace ATF.Repository.Mock.Internal
{
	internal static class QueryParametersExtractor
	{
		//private static string _dateTimePattern = @"([0-9]{4})-([0-9]{2})-([0-9]{2})T([0-9]{2}):([0-9]{2}):([0-9]{2}).([0-9]{3})";
		public static List<object> ExtractParameters(IBaseFilterableQuery selectQuery) {
			var parameters = new List<object>();
			ExtractParametersFromFilter(selectQuery.Filters, parameters);
			return parameters;
		}

		public static List<ExpectedColumnValueItem> ExtractColumnValues(IBaseQuery insertQuery) {
			var parameters = new List<ExpectedColumnValueItem>();
			insertQuery.ColumnValues.Items.ForEach(x => {
				if (TryExtractParametersParameterExpression(x.Value, out var value)) {
					parameters.Add(new ExpectedColumnValueItem() {
						Name = x.Key,
						Value = value
					});
				}
			});
			return parameters;
		}

		private static void ExtractParametersFromFilter(IFilter filter, List<object> parameters) {
			if (filter.FilterType == FilterType.FilterGroup) {
				ExtractParametersFromFilterGroup(filter, parameters);
			} else if (filter.FilterType == FilterType.CompareFilter || filter.FilterType == FilterType.InFilter) {
				ExtractParametersFromCompareFilter(filter, parameters);
			} else if (filter.FilterType == FilterType.IsNullFilter) {
				ExtractParametersFromIsNullFilter(filter, parameters);
			} else if (filter.FilterType == FilterType.Exists) {
				ExtractParametersFromExistsFilter(filter, parameters);
			}
		}

		private static void ExtractParametersFromExistsFilter(IFilter filter, List<object> parameters) {
			ExtractParametersFromFilterGroup(filter.SubFilters, parameters);
		}

		private static void ExtractParametersFromIsNullFilter(IFilter filter, List<object> parameters) {
			parameters.Add(null);
		}

		private static void ExtractParametersFromCompareFilter(IFilter filter, List<object> parameters) {
			ExtractParametersFromExpression(filter.LeftExpression, parameters);
			ExtractParametersFromExpression(filter.RightExpression, parameters);
			filter.RightExpressions?.ForEach(x=>ExtractParametersFromExpression(x, parameters));
		}

		private static void ExtractParametersFromExpression(IBaseExpression expression, List<object> parameters) {
			if (expression == null) {
				return;
			}
			if (expression.ExpressionType == EntitySchemaQueryExpressionType.Parameter) {
				if (TryExtractParametersParameterExpression(expression, out var value)) {
					parameters.Add(value);
				}
			}
		}

		private static bool TryExtractParametersParameterExpression(IBaseExpression expression, out object value) {
			if (expression.Parameter == null) {
				value = null;
				return false;
			}

			if (expression.Parameter.DataValueType == DataValueType.Date ||
			 expression.Parameter.DataValueType == DataValueType.Time ||
			 expression.Parameter.DataValueType == DataValueType.DateTime) {
				value = ExtractParametersParameterDateTimeExpression(expression);
				return true;
			}

			value = expression.Parameter.Value;
			return true;
		}

		private static object ExtractParametersParameterDateTimeExpression(IBaseExpression expression) {
			var rawData = expression.Parameter?.Value;
			return rawData is DateTime dateTime ? $"\"{dateTime:yyyy-MM-ddTHH:mm:ss.fff}\"" : rawData;
		}

		private static int ParseGroupValue(Match match, int index) {
			var rawValue = match.Groups[index].Value;
			return int.TryParse(rawValue, out var response) ? response : 0;
		}

		private static void ExtractParametersFromFilterGroup(IFilter filter, List<object> parameters) {
			filter.Items.ForEach(x=>ExtractParametersFromFilter(x.Value, parameters));
		}
	}
}
