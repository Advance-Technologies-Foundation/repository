using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using ATF.Repository;
using ATF.Repository.Attributes;
using ATF.Repository.Providers.BpModelParser;
using FluentAssertions;
using NUnit.Framework;

namespace ATF.Repository.UnitTests.Providers {
	public class ModelParserTests {

		private static readonly Func<string, Task<string>> GetJsonContent = fileName => 
			File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Providers","Samples",fileName));
		
		[TestCase("brokenJson.json")]
		[TestCase("error.json")]
		public async Task Parse_Should_Handle_Error(string fileName) {
			//Arrange
			string jsonContent = await GetJsonContent(fileName);
			
			//Act
			RunProcessResponseWrapper<ModelOne> result = ModelParser.Parse<ModelOne>(jsonContent);
			
			//Assert
			AssertOnError(result);
		}
		
		private static void AssertOnError<T>(RunProcessResponseWrapper<T> result ) where T : BaseBpModel {
			result.Success.Should().BeFalse();
			result.ProcessStatus.Should().Be(0, "process status should be 0 on error");
			result.ProcessId.Should<Guid>().Be(Guid.Empty);
			result.ResultModel.Should().BeNull("there is no model on error");
			result.ErrorInfo.Should().NotBeNull("there is must be an errorInfo when error");
		}
		private static void AssertOnSucces<T>(RunProcessResponseWrapper<T> result) where T : BaseBpModel {
			result.Success.Should().BeTrue("model should be parsed successfully");
			result.ErrorInfo.Should().BeNull("there is no errorInfo when success");
			result.ProcessStatus.Should().Be(2, "process status should be 2 on success");
			result.ProcessId.Should().NotBe(Guid.Empty, "processId should not be empty on success");
			result.ResultModel.Should().NotBeNull("there is a model on success");
		}
		
		
		private class ModelOne : BaseBpModel { }
		
		
		[TestCase("Process_1d72e4b.json")]
		public async Task Parse_Should_ParseModel(string fileName) {
			//Arrange
			string jsonContent = await GetJsonContent(fileName);
			
			//Act
			RunProcessResponseWrapper<ModelTwo> result = ModelParser.Parse<ModelTwo>(jsonContent);

			//Assert
			AssertOnSucces(result);
			
			result.ResultModel.OutputParam1.Should().Be(null, "OutputParam1 should be parsed correctly");
			
			DateTime expectedDate = DateTime.Parse("2025-05-25T18:25:23.3049978", CultureInfo.InvariantCulture, 
				DateTimeStyles.AssumeUniversal| DateTimeStyles.AdjustToUniversal);
			
			result.ResultModel.OutputDateTime!.HasValue.Should().BeTrue("OutputDateTime should have a value");
			result.ResultModel.OutputDateTime!.Value.Should().BeCloseTo(expectedDate, TimeSpan.FromSeconds(1),
				"OutputDateTime should be parsed correctly");
			
			result.ResultModel.OutputCollection2.Should().BeEmpty();
			
			result.ResultModel.OutputCollection.Should().NotBeNull("OutputCollection should not be null");
			result.ResultModel.OutputCollection.Should().HaveCount(7, "OutputCollection should have 7 items");
			
			Type[] ass = result.ResultModel.OutputCollection!.GetType().GetInterfaces();
			ass.Should().Contain(typeof(IEnumerable<SomeObject>));
				
			ICollection<SomeObject> collection = result.ResultModel.OutputCollection!;
			foreach (SomeObject someObject in collection) {
				someObject.Id.Should().NotBeNull().And.NotBeEmpty().And.NotBe(Guid.Empty);
				someObject.Name.Should().NotBeNull().And.NotBeEmpty();
				someObject.Name.Should().NotBeNull().And.NotBeEmpty();
				someObject.Number.Should().BeNull();
			}
		}
	}
}


[Schema("Process_1d72e4b")]
public class ModelTwo : BaseBpModel {
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
