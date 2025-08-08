using System.ComponentModel.DataAnnotations;

namespace EquipLink.ViewModels.CartVMs
{
    public class CheckoutRequestModel
    {
        [Required]
        public AddressModel BillingAddress { get; set; } = new();
        public AddressModel ShippingAddress { get; set; } = new();
        public string? BillingAddressSelect { get; set; }
        public string? ShippingAddressSelect { get; set; }
        public bool SameAsBilling { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public string PaymentMethod { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Payment notes cannot exceed 500 characters")]
        public string? PaymentNotes { get; set; }

        // Credit Card Properties with conditional validation
        [ConditionalRequired("PaymentMethod", "Credit Card", ErrorMessage = "Card number is required")]
        [RegularExpression(@"^\d{4}\s\d{4}\s\d{4}\s\d{4}$", ErrorMessage = "Card number must be 16 digits")]
        public string? CardNumber { get; set; }

        [ConditionalRequired("PaymentMethod", "Credit Card", ErrorMessage = "Expiry date is required")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Expiry date must be in MM/YY format")]
        public string? CardExpiry { get; set; }

        [ConditionalRequired("PaymentMethod", "Credit Card", ErrorMessage = "CVC is required")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVC must be 3-4 digits")]
        public string? CardCVC { get; set; }
    }

    // Custom validation attribute for conditional required fields
    public class ConditionalRequiredAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;
        private readonly string _targetValue;

        public ConditionalRequiredAttribute(string dependentProperty, string targetValue)
        {
            _dependentProperty = dependentProperty;
            _targetValue = targetValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_dependentProperty);
            if (property == null)
                return new ValidationResult($"Unknown property: {_dependentProperty}");

            var dependentValue = property.GetValue(validationContext.ObjectInstance)?.ToString();

            if (dependentValue == _targetValue)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required");
                }
            }

            return ValidationResult.Success;
        }
    }
}