using System.ComponentModel.DataAnnotations;

namespace EquipLink.ViewModels.AdminVMs
{
    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string UserFname { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string UserLname { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email Address")]
        public string UserEmail { get; set; } = null!;

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string? UserPhone { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        [Display(Name = "New Password")]
        public string? UserPassword { get; set; }

        [Compare("UserPassword", ErrorMessage = "Password and confirmation do not match")]
        [Display(Name = "Confirm New Password")]
        public string? UserPasswordConfirmation { get; set; }
    }
}
