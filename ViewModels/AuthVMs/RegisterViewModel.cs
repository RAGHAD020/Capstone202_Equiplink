using System.ComponentModel.DataAnnotations;

namespace EquipLink.ViewModels.AuthVMs
{
    public class RegisterViewModel : IValidatableObject
    {
        [Required]
        [StringLength(100)]
        public string UserFname { get; set; }

        [Required]
        [StringLength(100)]
        public string UserLname { get; set; }

        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "National ID must be 10 digits")]
        public string UserNationalId { get; set; }

        [StringLength(50)]
        public string UserPhone { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string UserPassword { get; set; }

        [Required]
        [Compare("UserPassword")]
        public string UserPasswordConfirmation { get; set; }

        [Required]
        public string UserType { get; set; }

        // Company fields
        public string CoName { get; set; }
        public string CoEmail { get; set; }
        public string CoPhone { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Tax Number must be 10 digits")]
        public string CoTaxNumber { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (UserType == "Provider")
            {
                if (string.IsNullOrEmpty(CoName))
                    yield return new ValidationResult("Company Name is required", new[] { nameof(CoName) });

                if (string.IsNullOrEmpty(CoEmail))
                    yield return new ValidationResult("Company Email is required", new[] { nameof(CoEmail) });

                if (string.IsNullOrEmpty(CoPhone))
                    yield return new ValidationResult("Company Phone is required", new[] { nameof(CoPhone) });

                if (string.IsNullOrEmpty(CoTaxNumber))
                    yield return new ValidationResult("Tax Number is required", new[] { nameof(CoTaxNumber) });
            }
        }
    }
}