using EquipLink.Models;
using System.ComponentModel.DataAnnotations;

namespace EquipLink.ViewModels.OperatorVMs
{
    public class ProfileUpdateViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        [StringLength(100, ErrorMessage = "First name must be between {2} and {1} characters", MinimumLength = 1)]
        public string User_FName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        [StringLength(100, ErrorMessage = "Last name must be between {2} and {1} characters", MinimumLength = 1)]
        public string User_LName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string User_Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone")]
        [StringLength(50, ErrorMessage = "Phone number must be between {2} and {1} characters", MinimumLength = 6)]
        public string User_Phone { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 8)]
        public string? User_Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("User_Password", ErrorMessage = "Passwords do not match")]
        public string? User_Password_confirmation { get; set; }

        public Company? Company { get; set; }
    }
}
