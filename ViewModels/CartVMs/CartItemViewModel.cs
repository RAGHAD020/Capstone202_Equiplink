using EquipLink.Models;

namespace EquipLink.ViewModels.CartVMs
{
    public class CartItemViewModel
    {
        public Equipment Equipment { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal DailyRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public Request? LatestRequest { get; set; } // Added
    }
}
