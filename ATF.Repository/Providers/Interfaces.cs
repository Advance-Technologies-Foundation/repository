namespace ATF.Repository.Providers
{
	using System;
	using Terrasoft.Core.Process;
	using System.Collections.Generic;

	public interface IDataProvider
	{
		IDefaultValuesResponse GetDefaultValues(string schemaName);

		IItemsResponse GetItems(ISelectQuery selectQuery);

		IExecuteResponse BatchExecute(List<IBaseQuery> queries);

		T GetSysSettingValue<T>(string sysSettingCode);

		bool GetFeatureEnabled(string featureCode);

		IExecuteProcessResponse ExecuteProcess(IExecuteProcessRequest request);
	}

	public interface IDefaultValuesResponse
	{
		bool Success { get; }

		Dictionary<string, object> DefaultValues { get; }

		string ErrorMessage { get; }
	}

	public interface IItemsResponse
	{
		bool Success { get; }

		List<Dictionary<string, object>> Items { get; }

		string ErrorMessage { get; }
	}

	public interface IExecuteItemResponse
	{
		bool Success { get; }

		int RowsAffected { get; }

		string ErrorMessage { get; }
	}

	public interface IExecuteResponse
	{
		bool Success { get; }
		List<IExecuteItemResponse> QueryResults { get;  }

		string ErrorMessage { get; }
	}

	public interface ISysSettingResponse<T>
	{
		bool Success { get; }

		T Value { get; }

		string ErrorMessage { get; }
	}

	public interface IFeatureResponse
	{
		bool Success { get; }

		bool Enabled { get; }

		string ErrorMessage { get; }
	}

	public interface IBusinessProcessResponse<T>
	{
		bool Success { get; }

		string ErrorMessage { get; }
		Guid ProcessId { get;  }
		ProcessStatus ProcessStatus { get; } 

		T Result { get; }
	}

	public interface IExecuteProcessRequest
	{
		string ProcessSchemaName { get; }
		Dictionary<string, string> InputParameters { get; }

		Dictionary<string, object> RawInputParameters { get; }

		List<IExecuteProcessRequestItem> ResultParameters { get; }
	}

	public interface IExecuteProcessRequestItem
	{
		string Code { get; }
		Type DataValueType { get; }
	}
	
	public interface IExecuteProcessResponse
	{
		bool Success { get; }
		string ErrorMessage { get; }
		Guid ProcessId { get; }
		ProcessStatus ProcessStatus { get; }
		Dictionary<string, object> ResponseValues { get; }
	}
}

