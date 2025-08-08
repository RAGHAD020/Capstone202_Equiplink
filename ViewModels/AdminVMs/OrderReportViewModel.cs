namespace EquipLink.ViewModels.AdminVMs
{
    public class OrderReportViewModel
    {
        public List<OrderStatusViewModel> OrdersByStatus { get; set; } = new();
        public int TotalOrders { get; set; }
    }
}
