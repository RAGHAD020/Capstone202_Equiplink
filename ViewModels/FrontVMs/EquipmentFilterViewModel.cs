namespace EquipLink.ViewModels.FrontVMs
{
    public class EquipmentFilterViewModel
    {
        public List<int>? Categories { get; set; }
        public List<string>? Conditions { get; set; }
        public List<string>? Availabilities { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; } // Added
        public int? Page { get; set; }
    }
}
