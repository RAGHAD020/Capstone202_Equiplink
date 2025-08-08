using System.ComponentModel.DataAnnotations;

namespace EquipLink.ViewModels.EquipmentVMs
{
    public class RequestCreateViewModel
    {
        [Required]
        [StringLength(1000)]
        [Display(Name = "Request Description")]
        public string ReqDescription { get; set; }

        [Display(Name = "Insurance Per Day")]
        [Range(0, double.MaxValue)]
        public decimal ReqInsurancePerDay { get; set; }
    }
}
