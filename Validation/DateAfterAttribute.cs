using System.ComponentModel.DataAnnotations;

namespace EquipLink.Validation
{
    public class DateAfterAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateAfterAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ErrorMessage = ErrorMessageString;
            var currentValue = (DateTime?)value;

            if (currentValue == null)
            {
                return ValidationResult.Success;
            }

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
            {
                throw new ArgumentException("Property with this name not found");
            }

            var comparisonValue = (DateTime?)property.GetValue(validationContext.ObjectInstance);

            if (comparisonValue == null)
            {
                return ValidationResult.Success;
            }

            if (currentValue <= comparisonValue)
            {
                return new ValidationResult($"Must be after {_comparisonProperty}");
            }

            return ValidationResult.Success;
        }
    }
}
