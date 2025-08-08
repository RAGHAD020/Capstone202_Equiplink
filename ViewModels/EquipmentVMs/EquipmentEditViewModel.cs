using System.ComponentModel.DataAnnotations;

namespace EquipLink.ViewModels.EquipmentVMs
{
    public class EquipmentEditViewModel
    {
        public int EquId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Equipment Name")]
        public string EquName { get; set; }

        [Display(Name = "Description")]
        public string EquDescription { get; set; }

        [Required]
        [Display(Name = "Condition")]
        public string EquCondition { get; set; }

        [Required]
        [Display(Name = "Availability Status")]
        public string EquAvailabilityStatus { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a positive number")]
        [Display(Name = "Quantity")]
        public int EquQuantity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        [Display(Name = "Price")]
        public decimal EquPrice { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategId { get; set; }

        [Required]
        [Display(Name = "Equipment Type")]
        public string EquType { get; set; } = "rent"; // Added this property with default value

        [Display(Name = "Equipment Image")]
        public IFormFile EquImage { get; set; }

        [Display(Name = "Current Image")]
        public string CurrentImagePath { get; set; }

        //Added
        [Display(Name = "Model")]
        public string? EquModel { get; set; }

        [Display(Name = "Model Year")]
        [Range(1900, 2100)]
        public int? EquModelYear { get; set; }

        [Display(Name = "Brand")]
        public string? EquBrand { get; set; }

        [Display(Name = "Working Hours")]
        [Range(0, int.MaxValue)]
        public int? EquWorkingHours { get; set; }

        [Display(Name = "Features")]
        public string? EquFeatures { get; set; }
    }
}
