namespace ATF.Repository.Mock
{
	using System;
	using System.Collections.Generic;

	public interface IDataStore
	{
		bool EmulateSystemColumnsBehavior { get; set; }
		void RegisterModelSchema<T>() where T : BaseModel;
		void RegisterModelSchema(params Type[] types);
		void SetDefaultValues<T>(Action<T> action) where T : BaseModel, new();
		T AddModel<T>(Action<T> action) where T : BaseModel, new();
		T AddModel<T>(Guid recordId, Action<T> action) where T : BaseModel, new();
		void AddModelRawData<T>(List<Dictionary<string, object>> recordList) where T : BaseModel;
		void AddModelRawData(string schemaName, List<Dictionary<string, object>> recordList);
		void LoadDataFromFileStore(string folderPath);
	}
}
