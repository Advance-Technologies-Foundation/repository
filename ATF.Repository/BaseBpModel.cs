using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using ATF.Repository.Attributes;

namespace ATF.Repository {
	public abstract class BaseBpModel : IValidatableObject {
		protected BaseBpModel() {
			SchemaAttribute attr = GetType().GetCustomAttribute<SchemaAttribute>();
			SchemaName = attr.Name;
			bool isGuid = Guid.TryParse(attr.UId, out Guid uid); 
			SchemaUId = isGuid ? (Guid?)uid : null; 
			
			ValidationContext ctx = new ValidationContext(this);
			IEnumerable<ValidationResult> errors = Validate(ctx);
			IEnumerable<ValidationResult> validationResults = errors as ValidationResult[] ?? errors.ToArray();
			if(validationResults.Any()) {
				throw new ValidationException(
					$"Validation failed for the business process model ({ctx.DisplayName}).",
					new ValidationException(validationResults?.FirstOrDefault()?.ErrorMessage));
			}
			
		}

		internal string SchemaName { get; }

		internal Guid? SchemaUId { get;}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			
			List<ValidationResult> errors = new List<ValidationResult>();
			BaseBpModel obj = validationContext.ObjectInstance as BaseBpModel;
			
			if(string.IsNullOrWhiteSpace(obj.SchemaName) && (obj.SchemaUId == null || obj.SchemaUId == Guid.Empty)) {
				errors.Add(new ValidationResult(
					"SchemaName and SchemaUId cannot be both null or empty", 
					new[] { nameof(SchemaName), nameof(SchemaUId) }));
			}
			
			return errors;
		}

	}
}
