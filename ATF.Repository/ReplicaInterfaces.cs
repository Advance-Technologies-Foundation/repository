namespace ATF.Repository
{
	using System;
	using System.Collections.Generic;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;
	using QueryType = Terrasoft.Nui.ServiceModel.DataContract.QueryType;
	using FilterType = Terrasoft.Nui.ServiceModel.DataContract.FilterType;
	using FunctionType = Terrasoft.Nui.ServiceModel.DataContract.FunctionType;
	using DataValueType = Terrasoft.Nui.ServiceModel.DataContract.DataValueType;
	using QuerySource = Terrasoft.Nui.ServiceModel.DataContract.QuerySource;

	public interface ISelectQueryColumns
	{
		Dictionary<string, ISelectQueryColumn> Items { get; set; }
	}

	public interface IFilter
	{
		LogicalOperationStrict LogicalOperation { get; }

		bool IsEnabled { get; }

		FilterType FilterType { get; }

		FilterComparisonType ComparisonType { get; }

		bool IsNull { get; }

		bool IsNot { get; }

		IFilterGroup SubFilters { get; }

		Dictionary<string, IFilter> Items { get; }

		IBaseExpression LeftExpression { get; }

		IBaseExpression RightExpression { get; }

		IBaseExpression[] RightExpressions { get; }

		bool TrimDateTimeParameterToDate { get; }
	}

	public interface IFilterGroup: IFilter
	{
		string RootSchemaName { get; }
	}

	public interface IBaseQueryColumn
	{
		IColumnExpression Expression { get; }
	}

	public interface ISelectQueryColumn: IBaseQueryColumn
	{
		OrderDirection OrderDirection { get; }
		int OrderPosition { get; }
	}

	public interface IBaseQueryColumns
	{
		Dictionary<string, IColumnExpression> Items { get; }
	}

	public interface IBaseQuery
	{
		string TypeName { get; }

		string RootSchemaName { get; }

		QueryKind QueryKind { get; }

		IBaseQueryColumns ColumnValues { get;  }

		bool IncludeProcessExecutionData { get; }
	}

	public interface IBaseFilterableQuery: IBaseQuery
	{
		QueryType QueryType { get; }

		IFilterGroup Filters { get; }
	}

	public interface ISelectQuery: IBaseFilterableQuery
	{
		ISelectQueryColumns Columns { get; }

		bool AllColumns { get; }

		bool IsDistinct { get; }

		int RowCount { get; }

		int ChunkSize { get; }

		int RowsOffset { get; }

		bool IsPageable { get; }

		bool UseLocalization { get; }

		bool UseRecordDeactivation { get; }

		bool QueryOptimize { get; }

		bool UseMetrics { get; }

		AdminUnitRoleSources AdminUnitRoleSources { get; }

		QuerySource QuerySource { get; }

		bool IgnoreDisplayValues { get; }
	}

	public interface IInsertQuery : IBaseQuery
	{

	}

	public interface IUpdateQuery : IBaseFilterableQuery
	{
		bool IsForceUpdate { get; }
	}

	public interface IDeleteQuery : IBaseFilterableQuery
	{
	}

	public interface IBatchQuery
	{
		List<IBaseQuery> Queries { get; }

		bool ContinueIfError { get; }

		Guid InstanceId { get; set; }

		bool IncludeProcessExecutionData { get; }
	}

	public interface IParameter
	{
		DataValueType DataValueType { get; }

		object Value { get; }
	}

	public interface IColumnExpression: IBaseExpression
	{
	}

	public interface IBaseExpression
	{
		EntitySchemaQueryExpressionType ExpressionType { get; }

		string ColumnPath { get; }

		IParameter Parameter { get; }

		FunctionType FunctionType { get; }

		IBaseExpression FunctionArgument { get; }

		IBaseExpression[] FunctionArguments { get; }

		AggregationType AggregationType { get; }

		IFilterGroup SubFilters { get; }

		ArithmeticOperation ArithmeticOperation { get; }

		IBaseExpression LeftArithmeticOperand { get; }

		IBaseExpression RightArithmeticOperand { get; }
	}
}
