using System;
using System.Collections.Generic;
using ATF.Repository.Attributes;
using ATF.Repository.Providers;

namespace ATF.Repository.TestClient;

public class Program {

	private const string ApplicationUrl = "https://k_krylov_nb.tscrm.com:40020";
	private const string Username = "Supervisor";
	private const string Password = "Supervisor";
	public static void Main(string[] args) {
		IDataProvider dataProvider = new RemoteDataProvider(ApplicationUrl, Username, Password);
		IAppDataContext? ctx = AppDataContextFactory.GetAppDataContext(dataProvider);
		ProcessModel model = new () {
			InputParam1 = "Test Input"
		};
		RunProcessResponseWrapper<ProcessModel>? result = ctx.RunProcess(model);
		Console.WriteLine($"Output: {result.ResultModel.OutputParam1}");
		Console.WriteLine($"Output: {result.ResultModel.OutputDateTime}");
	}

}

[Schema("Process_1d72e4b")]
public class ProcessModel : BaseBpModel
{
	[ProcessParameter("InputParam1", ProcessParameterDirection.Input)]
	public string? InputParam1 { get; set; }
	
	[ProcessParameter("OutputParam1", ProcessParameterDirection.Output)]
	public string? OutputParam1 { get; set; }
	
	[ProcessParameter("OutputDateTime", ProcessParameterDirection.Output)]
	public DateTime? OutputDateTime { get; set; }
	
	[ProcessParameter("OutputCollection", ProcessParameterDirection.Output)]
	public ICollection<SomeObject>? OutputCollection { get; set; }
	
	[ProcessParameter("OutputCollectionTwo", ProcessParameterDirection.Output)]
	public IEnumerable<SomeObject>? OutputCollection2 { get; set; }
}

public class SomeObject {
	public Guid? Id { get; set; }
	public string? Name { get; set; }
	public int? Number { get; set; }

}
