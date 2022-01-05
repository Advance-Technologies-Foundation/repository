namespace ATF.Repository
{
	using System.Reflection;
	using ATF.Repository.Mapping;

	internal interface ILazyModelPropertyManager
	{
		void LoadLazyProperty(BaseModel model, ModelItem propertyInfo);

		void SetLazyProperty(BaseModel model, PropertyInfo propertyInfo, object value);

		object GetLazyProperty(BaseModel model, PropertyInfo propertyInfo);
	}
}
