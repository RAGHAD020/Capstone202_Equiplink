using EquipLink.Models;

namespace EquipLink.ViewModels.CustomerVMs
{
    public class CustomerProfileViewModel
    {
        public User User { get; set; } = new User();
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<Address> Addresses { get; set; } = new List<Address>();

        // Form view models for the various forms in the view
        public UpdateProfileViewModel UpdateProfile { get; set; } = new UpdateProfileViewModel();
        public AddAddressViewModel AddAddress { get; set; } = new AddAddressViewModel();
        public UpdateAddressViewModel UpdateAddress { get; set; } = new UpdateAddressViewModel();
        public RequestMaintenanceViewModel RequestMaintenance { get; set; } = new RequestMaintenanceViewModel();
        public AddReviewViewModel AddReview { get; set; } = new AddReviewViewModel();
    }
}
