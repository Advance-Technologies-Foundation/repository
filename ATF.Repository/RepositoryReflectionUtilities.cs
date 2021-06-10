using System;
using System.Linq;
using System.Reflection;
using Terrasoft.Nui.ServiceModel.DataContract;

namespace ATF.Repository
{
	internal static class RepositoryReflectionUtilities
	{
		internal static MethodInfo GetGenericMethod(Type type, string methodName, params Type[] genericTypes) {
			MethodInfo response = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
				.FirstOrDefault(method => method.Name == methodName && method.ContainsGenericParameters);
			return response?.MakeGenericMethod(genericTypes);
		}
	}
}
