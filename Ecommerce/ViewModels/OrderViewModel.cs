using System.ComponentModel.DataAnnotations;

namespace Ecommerce.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Please select a payment method.")]
        public string PaymentMethod { get; set; } = null!;
        public List<string> PaymentMethods { get; set; } = new List<string> { "CreditCard", "PayPal" };
        [Required] public ProfileViewModel Profile { get; set; } = null!;
        public List<CartItemViewModel> CartItems { get; set; } = null!;
        [Range(0, 999999.99)] public decimal TotalAmount { get; set; }
        public CheckoutViewModel() { CartItems = new List<CartItemViewModel>(); }
    }

    public class OrderConfirmationViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
    }

    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
    }
}
