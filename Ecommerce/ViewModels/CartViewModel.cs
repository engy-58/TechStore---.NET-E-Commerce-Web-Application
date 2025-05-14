using System.ComponentModel.DataAnnotations;

namespace Ecommerce.ViewModels
{
    public class CartViewModel
    {
        public int CartId { get; set; }
        public List<CartItemViewModel> CartItems { get; set; } = null!;
        public decimal Total { get; set; }
        public CartViewModel() { CartItems = new List<CartItemViewModel>(); }
    }
    public class CartItemViewModel
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
    }
}