using System.Collections.Generic;
using Terrasoft.Nui.ServiceModel.DataContract;

namespace ATF.Repository.Providers
{
	public interface IDataProvider
	{
		IDefaultValuesResponse GetDefaultValues(string schemaName);

		IItemsResponse GetItems(SelectQuery selectQuery);

		IExecuteResponse BatchExecute(List<BaseQuery> queries);
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
}

