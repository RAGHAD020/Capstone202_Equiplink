using EquipLink.Models;
using EquipLink.ViewModels.CustomerVMs;

namespace EquipLink.ViewModels.FrontVMs
{
    public class EquipmentShowViewModel
    {
        public Equipment Equipment { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public AddReviewViewModel ReviewForm { get; set; } = new();
    }
}
