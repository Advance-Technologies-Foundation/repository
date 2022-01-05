namespace ATF.Repository.Replicas
{
	using Newtonsoft.Json;
	using Terrasoft.Common;
	using Terrasoft.Core.Entities;
	using FunctionType = Terrasoft.Nui.ServiceModel.DataContract.FunctionType;

	internal class BaseExpressionReplica: IBaseExpression
	{
		[JsonProperty("expressionType")]
		public EntitySchemaQueryExpressionType ExpressionType { get; set; }

		[JsonProperty("columnPath")]
		public string ColumnPath { get; set; }

		[JsonProperty("parameter")]
		public IParameter Parameter { get; set; }

		[JsonProperty("functionType")]
		public FunctionType FunctionType { get; set; }

		[JsonProperty("functionArgument")]
		public IBaseExpression FunctionArgument { get; set; }

		[JsonProperty("functionArguments")]
		public IBaseExpression[] FunctionArguments { get; set; }

		[JsonProperty("aggregationType")]
		public AggregationType AggregationType { get; set; }

		[JsonProperty("subFilters")]
		public IFilterGroup SubFilters { get; set; }

		[JsonProperty("arithmeticOperation")]
		public ArithmeticOperation ArithmeticOperation { get; set; }

		[JsonProperty("leftArithmeticOperand")]
		public IBaseExpression LeftArithmeticOperand { get; set; }

		[JsonProperty("rightArithmeticOperand")]
		public IBaseExpression RightArithmeticOperand { get; set; }
	}
}
