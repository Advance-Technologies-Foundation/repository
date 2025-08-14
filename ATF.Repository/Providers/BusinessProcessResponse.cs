namespace ATF.Repository.Providers
{
	using System;
	using Terrasoft.Core.Process;

	internal class BusinessProcessResponse<T>: IBusinessProcessResponse<T>
	{
		public bool Success { get; set; }
		public string ErrorMessage { get; set; }
		public Guid ProcessId { get; set; }
		public ProcessStatus ProcessStatus { get; set; }
		public T Result { get; set; }
	}
}