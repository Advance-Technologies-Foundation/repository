namespace ATF.Repository.Mock.Internal
{
	using System.Data;

	#region Class: DataStoreExtensions

	internal static class DataStoreExtensions
	{

		#region Methods: Internal

		internal static LookupColumnMetaData GetLookupRelationship(this DataTable dataTable, string entityColumnName) {
			return dataTable.ExtendedProperties.ContainsKey(entityColumnName) &&
				dataTable.ExtendedProperties[entityColumnName] is LookupColumnMetaData lookupColumnMetaData
					? lookupColumnMetaData
					: null;
		}

		internal static void RegisterLookupRelationship(this DataTable dataTable, string columnName,
			string lookupSchemaName) {
			dataTable.ExtendedProperties.Add(columnName, new LookupColumnMetaData() {
				Name = columnName,
				ReferenceSchemaName = lookupSchemaName
			});
		}

		internal static void RegisterDetailRelationship(this DataTable dataTable, string detailPropertyName,
			string detailSchemaName, string detailColumnName, string masterColumnName) {
			dataTable.ExtendedProperties.Add(detailPropertyName, new DetailListMetaData() {
				PropertyName = detailPropertyName,
				DetailSchemaName = detailSchemaName,
				DetailColumnName = detailColumnName,
				MasterColumnName = masterColumnName
			});
		}

		#endregion

	}

	#endregion

}
