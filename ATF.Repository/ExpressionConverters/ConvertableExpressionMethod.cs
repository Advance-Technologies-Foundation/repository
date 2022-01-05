namespace ATF.Repository.ExpressionConverters
{
	using System.Collections.Generic;

	internal static class AvailableChainMethods
	{
		internal static List<string> MainMethods = new List<string>() {
			ConvertableExpressionMethod.Skip,
			ConvertableExpressionMethod.Take,
			ConvertableExpressionMethod.Where,
			ConvertableExpressionMethod.OrderBy,
			ConvertableExpressionMethod.OrderByDescending,
			ConvertableExpressionMethod.ThenBy,
			ConvertableExpressionMethod.ThenByDescending,
			ConvertableExpressionMethod.Any,
			ConvertableExpressionMethod.Count,
			ConvertableExpressionMethod.First,
			ConvertableExpressionMethod.FirstOrDefault,
			ConvertableExpressionMethod.Min,
			ConvertableExpressionMethod.Max,
			ConvertableExpressionMethod.Average,
			ConvertableExpressionMethod.Sum
		};

		internal static List<string> DetailMethods = new List<string>() {
			ConvertableExpressionMethod.Where,
			ConvertableExpressionMethod.Any,
			ConvertableExpressionMethod.Count,
			ConvertableExpressionMethod.Min,
			ConvertableExpressionMethod.Max,
			ConvertableExpressionMethod.Average,
			ConvertableExpressionMethod.Sum
		};
	}
	internal static class ConvertableExpressionMethod
	{
		public const string Skip = "Skip";
		public const string Take = "Take";
		public const string Where = "Where";
		public const string OrderBy = "OrderBy";
		public const string OrderByDescending = "OrderByDescending";
		public const string ThenBy = "ThenBy";
		public const string ThenByDescending = "ThenByDescending";
		public const string Any = "Any";
		public const string Count = "Count";
		public const string First = "First";
		public const string FirstOrDefault = "FirstOrDefault";
		public const string Min = "Min";
		public const string Max = "Max";
		public const string Average = "Average";
		public const string Sum = "Sum";

		public const string Contains = "Contains";
		public const string StartsWith = "StartsWith";
		public const string EndsWith = "EndsWith";

		public const string Select = "Select";
	}

	internal enum AvailableFieldMethod
	{
		None,
		In
	}

	internal enum AvailableColumnMethod
	{
		None,
		StartWith,
		EndWith,
		Contains
	}


}
