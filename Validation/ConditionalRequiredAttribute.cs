using System.ComponentModel.DataAnnotations;

namespace EquipLink.Validation
{
    public class ConditionalRequiredAttribute : ValidationAttribute
    {
        private readonly string _propertyName;
        private readonly object _desiredValue;

        public ConditionalRequiredAttribute(string propertyName, object desiredValue)
        {
            _propertyName = propertyName;
            _desiredValue = desiredValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance;
            var type = instance.GetType();
            var propertyValue = type.GetProperty(_propertyName)?.GetValue(instance, null);

            if (propertyValue?.ToString() == _desiredValue?.ToString() && value == null)
            {
                return new ValidationResult(ErrorMessage ?? $"This field is required when {_propertyName} is {_desiredValue}.");
            }

            return ValidationResult.Success;
        }
    }
}
