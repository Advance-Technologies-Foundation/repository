using System;
using System.ComponentModel.DataAnnotations;
using ATF.Repository.Attributes;
using ATF.Repository.Providers;
using FluentAssertions;
using NUnit.Framework;
using Terrasoft.Core;

namespace ATF.Repository.UnitTests {
	
	[Category("BusinessProcess")]
	[TestFixture]
	public class BaseBpModelTests {

		[Test]
		public void BaseBpModel_CanGet_SchemaName_FromAttribute() {
			FakeBaseBpModel model = new FakeBaseBpModel();
			model.SchemaName.Should()
				.Be("ProcessName_Here", 
					"SchemaAttribute can be created with only Process Name");
			model.SchemaUId.Should().BeNull( 
				"SchemaAttribute can be created with only Process Name, UId should be empty");
			
			
			FakeBaseBpModel_Name model2 = new FakeBaseBpModel_Name();
			model2.SchemaName.Should()
				.Be("ProcessName_Here", 
					"SchemaAttribute can be created with only Process Name");
			model2.SchemaUId.Should().BeNull( 
				"SchemaAttribute can be created with only Process Name, UId should be empty");
			
		}
		
		[Test]
		public void BaseBpModel_CanGet_SchemaUId_FromAttribute() {
			FakeBaseBpModel_Name_Schema model = new FakeBaseBpModel_Name_Schema();
			
			model.SchemaName.Should()
				.Be("ProcessName_Here", 
					"SchemaAttribute can be created with Process UId and Process Name");
			
			model.SchemaUId.Should()
				.Be(Guid.Parse("C05A4282-9CFB-4B40-B449-DB63633E8FD2"),
				"SchemaAttribute can be created with Process UId and Process Name");
		}

		[Test]
		public void BaseBpModel_Throws_When_NameIsGuid() {
			Action act = () => {
				FakeBaseBpModel4 model = new FakeBaseBpModel4();
			};
			act.Should().Throw<ValidationException>()
				.WithMessage("Validation failed for the business process model (FakeBaseBpModel4)*");
		}
	}
	
	[Schema("ProcessName_Here")]
	public class FakeBaseBpModel : BaseBpModel { }
	
	[Schema(Name = "ProcessName_Here")]
	public class FakeBaseBpModel_Name : BaseBpModel { }
	
	
	[Schema("ProcessName_Here", "C05A4282-9CFB-4B40-B449-DB63633E8FD2")]
	public class FakeBaseBpModel_Name_Schema : BaseBpModel { }
	
	[Schema(Name = "", UId = "")]
	public class FakeBaseBpModel4 : BaseBpModel { }
	
	
}

