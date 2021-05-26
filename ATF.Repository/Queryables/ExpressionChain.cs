using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ATF.Repository.Queryables
{
	internal class ExpressionChainDtoType
	{
		internal Type Type { get; set; }
		internal bool IsTypeFromGeneric { get; set; }

		internal bool Equals(ExpressionChainDtoType another) {
			return Type == another.Type && IsTypeFromGeneric == another.IsTypeFromGeneric;
		}
	}

	internal class ExpressionChain : List<ExpressionChainItem>
	{
		internal Type GetModelType() {
			return this.OrderBy(x=>x.Position).First().InputDtoType.Type;
		}

		internal Type OutputAppliedType() {
			return this.Any(x => x.IsAppliedToQuery)
				? this.Where(x => x.IsAppliedToQuery).OrderBy(x => x.Position).Last().OutputDtoType.Type
				: GetModelType();
		}
	}

	internal class ExpressionChainItem
	{
		internal int Position { get; set; }

		internal MethodCallExpression Expression { get; }

		internal bool IsAppliedToQuery { get; set; }
		internal ExpressionChainDtoType InputDtoType { get; }
		internal ExpressionChainDtoType OutputDtoType { get; }

		internal ExpressionChainItem(MethodCallExpression expression) {
			Expression = expression;
			InputDtoType = GenerateExpressionChainDtoType(expression.Arguments.First().Type);
			OutputDtoType = GenerateExpressionChainDtoType(expression.Type);
		}

		private static ExpressionChainDtoType GenerateExpressionChainDtoType(Type expressionType) {
			var genericArguments = expressionType.GetGenericArguments();
			var dtoType = genericArguments.Any()
				? genericArguments.First()
				: expressionType;
			return new ExpressionChainDtoType() {
				IsTypeFromGeneric = dtoType != expressionType,
				Type = dtoType
			};
		}
	}
}
