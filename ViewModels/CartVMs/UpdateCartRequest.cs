using System.ComponentModel.DataAnnotations;

namespace EquipLink.ViewModels.CartVMs
{
    public class UpdateCartRequest
    {
        [Required(ErrorMessage = "Equipment ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid equipment ID")]
        public int EquipmentId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
