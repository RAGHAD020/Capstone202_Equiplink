using EquipLink.Models;

namespace EquipLink.ViewModels.FrontVMs
{
    public class HomeViewModel
    {
        public List<Category> Categories { get; set; } = new();
        public List<Equipment> FeaturedEquipment { get; set; } = new();
    }
}
