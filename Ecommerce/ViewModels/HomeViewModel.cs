namespace Ecommerce.ViewModels
{
    public class HomeViewModel
    {
        public List<ProductViewModel> FeaturedProducts { get; set; } = new List<ProductViewModel>();
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}


