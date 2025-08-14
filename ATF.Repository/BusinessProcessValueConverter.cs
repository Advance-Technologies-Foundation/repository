namespace ATF.Repository
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;

	#region Class: BusinessProcessValueConverter

	internal static class BusinessProcessValueConverter
	{

		#region Methods: Public

		public static bool TrySerializeProcessValue(Type type, object value, out string serializedValue) {
			serializedValue = string.Empty;
			if (type == typeof(int) && value is int intValue) {
				serializedValue = intValue.ToString();
				return true;
			}
			if (type == typeof(decimal) && value is decimal decimalValue) {
				serializedValue = decimalValue.ToString(CultureInfo.InvariantCulture);
				return true;
			}
			if (type == typeof(float) && value is float floatValue) {
				serializedValue = floatValue.ToString(CultureInfo.InvariantCulture);
				return true;
			}
			
			if (type == typeof(double) && value is double doubleValue) {
				serializedValue = doubleValue.ToString(CultureInfo.InvariantCulture);
				return true;
			}

			if (type == typeof(bool) && value is bool boolValue) {
				serializedValue = boolValue.ToString();
				return true;
			}
			
			if (type == typeof(long) && value is long longValue) {
				serializedValue = longValue.ToString();
				return true;
			}
			
			if (type == typeof(ulong) && value is ulong ulongValue) {
				serializedValue = ulongValue.ToString();
				return true;
			}
			
			if (type == typeof(Guid) && value is Guid guidValue && guidValue != Guid.Empty) {
				serializedValue = guidValue.ToString();
				return true;
			}
			
			if (type == typeof(DateTime) && value is DateTime dateTimeValue && dateTimeValue != DateTime.MinValue) {
				serializedValue = dateTimeValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
				return true;
			}
			
			if (type == typeof(string) && value is string stringValue) {
				serializedValue = stringValue;
				return true;
			}

			return false;
		}

		#endregion

	}

	#endregion
}