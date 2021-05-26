using System.Reflection;
using ATF.Repository.Mapping;

namespace ATF.Repository
{
	internal interface ILazyModelPropertyLoader
	{
		void LoadLazyProperty(BaseModel model, ModelItem propertyInfo);
	}
}
