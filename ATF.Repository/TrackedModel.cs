namespace ATF.Repository
{
	internal class TrackedModel<T>: ITrackedModel<T> where T : BaseModel
	{
		public T Model { get; internal set; }
	}
}