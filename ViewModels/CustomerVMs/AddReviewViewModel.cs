using System.ComponentModel.DataAnnotations;

namespace EquipLink.ViewModels.CustomerVMs
{
    public class AddReviewViewModel
    {
        [Required(ErrorMessage = "Please select a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please enter a comment")]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Comment { get; set; } = string.Empty;

        public int EquipmentId { get; set; }
        public int OrderId { get; set; }
    }
}
