using Ecommerce.Data;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> List(int? categoryId, int page = 1)
        {
            int pageSize = 8;
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query.ToListAsync();

            var productViewModels = products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                CategoryName = p.Category?.Name,
                StockQuantity = p.StockQuantity,
                SKU = p.SKU,
                Brand = p.Brand
            }).ToList();

            var totalProducts = productViewModels.Count;
            var totalPages = (int)System.Math.Ceiling(totalProducts / (double)pageSize);
            var paginatedProducts = productViewModels
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new ProductListViewModel
            {
                Products = paginatedProducts,
                CurrentPage = page,
                TotalPages = totalPages,
                CategoryId = categoryId
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var productViewModel = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                CategoryName = product.Category?.Name,
                StockQuantity = product.StockQuantity,
                SKU = product.SKU,
                Brand = product.Brand,
                Reviews = product.Reviews?.Select(r => new ReviewViewModel
                {
                    UserName = r.User?.FullName,
                    Comment = r.Comment,
                    Rating = r.Rating,
                    ReviewDate = r.ReviewDate
                }).ToList() ?? new List<ReviewViewModel>()
            };

            return View(productViewModel);
        }
    }
}