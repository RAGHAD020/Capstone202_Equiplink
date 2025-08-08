using EquipLink.Models;

namespace EquipLink.ViewModels.EquipmentVMs
{
    public class EquipmentRequestsViewModel
    {
        public Equipment Equipment { get; set; }
        public List<Request> Requests { get; set; } = new();
    }
}
