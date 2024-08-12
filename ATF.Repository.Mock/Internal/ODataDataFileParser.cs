namespace ATF.Repository.Mock.Internal
{
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	#region Class: ODataDataFileParser

	internal class ODataDataFileParser: BaseDataFileParser
	{
		#region Constants: Private

		private const string SchemaNamePropertyName = "@odata.context";
		private const string ValuePropertyName = "value";
		private const string SchemaPropertySplitKey = "$metadata#";

		#endregion

		#region Methods: Private

		private bool TryFetchRecords(JObject jDocument, out List<Dictionary<string, object>> records) {
			records = jDocument.TryGetValue(ValuePropertyName, out var values) && values is JArray jArray
				? ParseRecords(jArray)
				: null;
			return records != null;
		}

		private List<Dictionary<string, object>> ParseRecords(JArray jArray) {
			var response = new List<Dictionary<string, object>>();
			foreach (var item in jArray) {
				if (item is JObject jObjectItem) {
					response.Add(ParseRecord(jObjectItem));
				}
			}

			return response;
		}

		private Dictionary<string, object> ParseRecord(JObject jObjectItem) {
			var record = new Dictionary<string, object>();
			foreach (var jProperty in jObjectItem.Properties()) {
				if (jProperty.Value is JValue jValue) {
					record.Add(jProperty.Name, jValue.Value);
				}
			}
			return record;
		}

		private bool TryFetchSchemaName(JObject jDocument, out string schemaName) {
			schemaName = jDocument.TryGetValue(SchemaNamePropertyName, out var value)
				? ParseSchemaName(value)
				: string.Empty;

			return !string.IsNullOrEmpty(schemaName);
		}

		private string ParseSchemaName(JToken jToken) {
			var value = jToken.Value<string>();
			if (string.IsNullOrEmpty(value)) {
				return value;
			}

			var parts = value.Split(new string[] {SchemaPropertySplitKey}, StringSplitOptions.None);
			if (parts.Length != 2) {
				return string.Empty;
			}
			return parts[1];
		}

		#endregion

		#region Methods: Public

		public bool TryParse(string path, out DataFileDto dto) {
			dto = null;
			try {
				var fileContent = GetFileContent(path);
				var document = JsonConvert.DeserializeObject(fileContent);
				if (document is JObject jDocument && TryFetchSchemaName(jDocument, out var schemaName) &&
					TryFetchRecords(jDocument, out var records)) {
					dto = new DataFileDto() {
						SchemaName = schemaName,
						Records = records
					};
				}
			} catch (Exception e) {
				throw;
			}
			return dto != null;
		}

		#endregion
	}

	#endregion

}