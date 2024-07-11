namespace ATF.Repository.Mock.Internal
{
	using System;
	using Terrasoft.Nui.ServiceModel.DataContract;

	internal static class ValueBuilder
	{
		internal static object GetActualValue(IParameter parameter) {
			var p = new Parameter() {
				Value = parameter.Value,
				DataValueType = parameter.DataValueType
			};
			var value = p.GetValue(null);
			if (parameter.DataValueType.GetValueType() == typeof(DateTime) && value is DateTime dateTimeValue) {
				return new DateTime(dateTimeValue.Year, dateTimeValue.Month, dateTimeValue.Day, dateTimeValue.Hour,
					dateTimeValue.Minute, dateTimeValue.Second);
			}
			return value;
		}
	}
}
