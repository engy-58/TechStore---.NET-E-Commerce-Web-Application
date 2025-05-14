namespace Ecommerce.ViewModels
{
    public class ReviewViewModel
    {
        public string? UserName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = null!;
        public DateTime ReviewDate { get; set; }
    }
}
