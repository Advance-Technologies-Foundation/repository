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

		internal ExpressionChainDtoType(Type type) {
			var genericArguments = type.GetGenericArguments();
			var dtoType = genericArguments.Any()
				? genericArguments.First()
				: type;
			IsTypeFromGeneric = dtoType != type;
			Type = dtoType;
		}

		internal bool Equals(ExpressionChainDtoType another) {
			return Type == another.Type && IsTypeFromGeneric == another.IsTypeFromGeneric;
		}
	}

}
