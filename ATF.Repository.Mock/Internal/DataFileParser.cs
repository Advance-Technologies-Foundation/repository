namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;

	internal static class DataFileParser
	{
		private static ODataDataFileParser _oDataDataFileParser;

		private static ODataDataFileParser ODataDataFileParser =>
			_oDataDataFileParser ?? (_oDataDataFileParser = new ODataDataFileParser());

		public static bool TryParse(string path, out DataFileDto dto) {
			if (ODataDataFileParser.TryParse(path, out var oDataDto)) {
				dto = oDataDto;
				return true;
			}

			throw new NotImplementedException($"Cannot parse file by path: {path}");
		}
	}

	internal class DataFileDto
	{
		public string SchemaName { get; set; }
		public List<Dictionary<string, object>> Records { get; set; }
	}
}