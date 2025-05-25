using Terrasoft.Core.ServiceModelContract;

namespace ATF.Repository.Providers
{
	using System.Collections.Generic;

	public interface IDataProvider
	{
		IDefaultValuesResponse GetDefaultValues(string schemaName);

		IItemsResponse GetItems(ISelectQuery selectQuery);

		IExecuteResponse BatchExecute(List<IBaseQuery> queries);

		T GetSysSettingValue<T>(string sysSettingCode);

		bool GetFeatureEnabled(string featureCode);
		
		RunProcessResponseWrapper<T> RunProcess<T>(T model) where T: BaseBpModel, new();
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
}

