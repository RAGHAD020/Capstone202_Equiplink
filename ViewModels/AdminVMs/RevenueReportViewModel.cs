namespace EquipLink.ViewModels.AdminVMs
{
    public class RevenueReportViewModel
    {
        public List<RevenueByMonthViewModel> RevenueByMonth { get; set; } = new();
        public decimal TotalRevenue { get; set; }
    }
}
