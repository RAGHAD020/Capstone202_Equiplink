namespace EquipLink.ViewModels.CartVMs
{
    public class CartViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; } = new();
        public decimal Total { get; set; }
    }
}
