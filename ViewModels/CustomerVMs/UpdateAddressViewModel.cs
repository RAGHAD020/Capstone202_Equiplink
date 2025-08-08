namespace EquipLink.ViewModels.CustomerVMs
{
    public class UpdateAddressViewModel
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string Country { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

}
