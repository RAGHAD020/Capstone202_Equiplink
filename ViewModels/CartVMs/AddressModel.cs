using System.ComponentModel.DataAnnotations;

namespace EquipLink.ViewModels.CartVMs
{
    public class AddressModel
    {
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string? FirstName { get; set; }

        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string? LastName { get; set; }

        [StringLength(255, ErrorMessage = "Street address cannot exceed 255 characters")]
        public string Street { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        public string State { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string PostalCode { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string Country { get; set; } = string.Empty;

        public bool IsDefault { get; set; }
    }

}
