using System;
using System.Collections.Generic;
using Terrasoft.Nui.ServiceModel.DataContract;

namespace ATF.Repository
{
	internal class DataValueTypeUtilities
	{
		private static readonly Dictionary<Type, DataValueType> ConvertDataValueTypeData = new Dictionary<Type, DataValueType>() {
			{typeof(string), DataValueType.Text},
			{typeof(int), DataValueType.Integer},
			{typeof(decimal), DataValueType.Float2},
			{typeof(DateTime), DataValueType.DateTime},
			{typeof(bool), DataValueType.Boolean},
			{typeof(Guid), DataValueType.Guid},
		};

		internal static bool IsGuidDataValueType(DataValueType dataValueType) {
			return dataValueType == DataValueType.Guid || dataValueType == DataValueType.Lookup ||
			       dataValueType == DataValueType.Enum;
		}

		internal static bool IsDateDataValueType(DataValueType dataValueType) {
			return dataValueType == DataValueType.Date || dataValueType == DataValueType.Time ||
			       dataValueType == DataValueType.DateTime;
		}

		internal static DataValueType ConvertTypeToDataValueType(Type propertyType) {
			propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
			if (ConvertDataValueTypeData.ContainsKey(propertyType)) {
				return ConvertDataValueTypeData[propertyType];
			}

			if (ModelUtilities.IsModelType(propertyType)) {
				return DataValueType.Lookup;
			}

			throw new NotImplementedException();
		}

		internal static Type ConvertDataValueTypeToType(DataValueType dataValueType) {
			switch (dataValueType) {
				case DataValueType.Guid:
				case DataValueType.Enum:
				case DataValueType.Lookup:
					return typeof(Guid);
				case DataValueType.Boolean:
					return typeof(bool);
				case DataValueType.Date:
				case DataValueType.Time:
				case DataValueType.DateTime:
					return typeof(DateTime);
				case DataValueType.Float:
				case DataValueType.Float1:
				case DataValueType.Float2:
				case DataValueType.Float3:
				case DataValueType.Float4:
				case DataValueType.Float8:
				case DataValueType.Money:
					return typeof(decimal);
				case DataValueType.Integer:
					return typeof(int);
				case DataValueType.Text:
				case DataValueType.ShortText:
				case DataValueType.MediumText:
				case DataValueType.LongText:
				case DataValueType.MaxSizeText:
					return typeof(string);
				default:
					return null;
			}
		}
	}
}
