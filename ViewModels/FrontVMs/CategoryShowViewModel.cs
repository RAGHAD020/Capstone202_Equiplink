using EquipLink.Models;

namespace EquipLink.ViewModels.FrontVMs
{
    public class CategoryShowViewModel
    {
        public Category Category { get; set; } = new();
        public List<Equipment> Equipment { get; set; } = new();
    }
}
