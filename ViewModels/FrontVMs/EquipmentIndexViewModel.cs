using EquipLink.Models;

namespace EquipLink.ViewModels.FrontVMs
{
    public class EquipmentIndexViewModel
    {
        public List<Equipment> Equipment { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public EquipmentFilterViewModel Filters { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }
}
