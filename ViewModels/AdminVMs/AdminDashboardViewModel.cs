using EquipLink.Models;

namespace EquipLink.ViewModels.AdminVMs
{
    public class AdminDashboardViewModel
    {
        public int TotalProviders { get; set; }
        public int ActiveUsers { get; set; }
        public int PendingRequests { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<User> RecentProviders { get; set; } = new();
        public List<int> ProviderActivity { get; set; } = new();
        public List<int> UserTypeDistribution { get; set; } = new();
    }
}
