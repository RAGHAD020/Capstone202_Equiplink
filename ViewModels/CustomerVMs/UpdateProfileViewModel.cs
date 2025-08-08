using System.ComponentModel.DataAnnotations;

namespace EquipLink.ViewModels.CustomerVMs
{
    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string UserFName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string UserLName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string UserEmail { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? UserPhone { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string? UserPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("UserPassword", ErrorMessage = "Passwords don't match")]
        public string? UserPasswordConfirmation { get; set; }

        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public string? CoName { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid company email address")]
        [StringLength(100, ErrorMessage = "Company email cannot exceed 100 characters")]
        public string? CoEmail { get; set; }

        [Phone(ErrorMessage = "Please enter a valid company phone number")]
        [StringLength(20, ErrorMessage = "Company phone number cannot exceed 20 characters")]
        public string? CoPhone { get; set; }

        [StringLength(10, MinimumLength = 10, ErrorMessage = "Tax number must be 10 digits")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Tax number must be 10 digits")]
        public string? CoTaxNumber { get; set; }

        [Required(ErrorMessage = "National ID is required")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "National ID must be 10 digits")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "National ID must be 10 digits")]
        public string UserNationalId { get; set; } = string.Empty;

       
    }
}