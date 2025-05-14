using System.ComponentModel.DataAnnotations;

namespace Ecommerce.ViewModels
{
    public class ProfileIndexViewModel
    {
        public ProfileViewModel Profile { get; set; } = null!;
        public List<OrderConfirmationViewModel> Orders { get; set; } = null!;
        public ProfileIndexViewModel() { Orders = new List<OrderConfirmationViewModel>(); }
    }

    public class ProfileViewModel
    {
        public string FullName { get; set; } = null!;
        public string ShippingAddress { get; set; } = null!;
        public string City { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }
}
