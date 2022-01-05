namespace ATF.Repository.Providers
{
	internal class SysSettingResponse<T>: ISysSettingResponse<T>
	{
		public bool Success { get; internal set; }
		public T Value { get; internal set; }
		public string ErrorMessage { get; internal set; }
	}
}
