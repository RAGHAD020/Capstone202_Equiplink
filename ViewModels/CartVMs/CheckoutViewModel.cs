using EquipLink.Models;
using System.ComponentModel.DataAnnotations;

namespace EquipLink.ViewModels.CartVMs
{
    public class CheckoutViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; } = new();
        public decimal Total { get; set; }
        public decimal TotalDeliveryFee { get; set; } = 0;
        public string? DeliveryMessage { get; set; }
        public List<Address> Addresses { get; set; } = new();

        // Only billing address is validated since that's what your form uses
        public AddressModel BillingAddress { get; set; } = new();

        // Remove validation from shipping address since it's not used in your form
        public AddressModel ShippingAddress { get; set; } = new();

        public string? BillingAddressSelect { get; set; }
        public string? ShippingAddressSelect { get; set; }
        public bool SameAsBilling { get; set; } = true; // Default to true since you're only using billing

        [Required(ErrorMessage = "Payment method is required")]
        public string PaymentMethod { get; set; } = "Credit Card";

        [StringLength(500, ErrorMessage = "Payment notes cannot exceed 500 characters")]
        public string? PaymentNotes { get; set; }

        public bool Ord_InstallationOperation { get; set; }

        [Required(ErrorMessage = "You must accept the terms and conditions")]
        public bool AcceptTerms { get; set; }

        public string? DeliveryMethod { get; set; }
        public string? DeliveryNotes { get; set; }

        // Credit card properties - only required when PaymentMethod is "Credit Card"
        public string? CardNumber { get; set; }
        public string? CardExpiry { get; set; }
        public string? CardCVC { get; set; }
    }
}