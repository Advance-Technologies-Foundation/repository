namespace ATF.Repository.Attributes
{
	using System;

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class SchemaAttribute: Attribute
	{
		public string Name { get; set; }
		public string UId { get; set; }
		
		public SchemaAttribute() {
			var a = "";
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="SchemaAttribute"/> class.
		/// <param name="name">Process Name as it appears in Creatio</param>
		/// </summary>
		public SchemaAttribute(string name) {
			bool isGuid = Guid.TryParse(name, out Guid _);
			if(isGuid) {
				throw new ArgumentOutOfRangeException(nameof(name), name, "Name must not be a valid Guid");
			}
			Name = name;
		}
		public SchemaAttribute(string name, string uId) : this(name: name) {
			bool isGuid = Guid.TryParse(uId, out Guid _);
			if(isGuid) {
				UId = uId;
				Name = name;
			}else {
				throw new ArgumentOutOfRangeException(nameof(uId), uId, "UId must be a valid Guid");
			}
		}

	}
	
	
	
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class ProcessParameterAttribute : Attribute {

		public ProcessParameterAttribute(string name, ProcessParameterDirection direction) {
			
			Name = name ?? throw new ArgumentNullException(nameof(name), "Parameter name cannot be null");
			if(string.IsNullOrWhiteSpace(name)) {
				throw new ArgumentException("Parameter name cannot be empty", nameof(name));
			}
			Direction = direction;
		}

		public string Name { get; set; }
		public ProcessParameterDirection Direction { get; set; }

	}
	
	public enum ProcessParameterDirection {
		Input = 0,
		Output = 1,
		Bidirectional = 2

	}
}
