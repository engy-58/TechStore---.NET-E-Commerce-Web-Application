namespace Ecommerce.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? CategoryName { get; set; }
        public string Brand { get; set; } = null!;
        public string SKU { get; set; } = null!;
        public int StockQuantity { get; set; }
        public List<ReviewViewModel> Reviews { get; set; } = null!;

        public ProductViewModel()
        {
            Reviews = new List<ReviewViewModel>();
        }
    }

    public class ProductListViewModel
    {
        public List<ProductViewModel> Products { get; set; } = null!;
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int? CategoryId { get; set; }

        public ProductListViewModel()
        {
            Products = new List<ProductViewModel>();
        }
    }
}
