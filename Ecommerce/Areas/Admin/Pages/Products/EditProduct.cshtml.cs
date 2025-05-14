using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Data;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class EditProductModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditProductModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public SelectList Categories { get; set; }

        public class InputModel
        {
            public int Id { get; set; }

            [Required]
            [StringLength(100)]
            public string Name { get; set; }

            [Required]
            [StringLength(500)]
            public string Description { get; set; }

            [Required]
            [Range(0.01, 10000)]
            public decimal Price { get; set; }

            [Required]
            [Range(0, 10000)]
            public int StockQuantity { get; set; }

            [Required]
            [StringLength(50)]
            public string SKU { get; set; }

            [Required]
            [StringLength(50)]
            public string Brand { get; set; }

            [Required]
            [StringLength(200)]
            public string ImageUrl { get; set; }

            [Required]
            public int CategoryId { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToPage("Index");
            }

            Input = new InputModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                SKU = product.SKU,
                Brand = product.Brand,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId
            };

            Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
                return Page();
            }

            var product = await _context.Products.FindAsync(Input.Id);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToPage("Index");
            }

            product.Name = Input.Name;
            product.Description = Input.Description;
            product.Price = Input.Price;
            product.StockQuantity = Input.StockQuantity;
            product.SKU = Input.SKU;
            product.Brand = Input.Brand;
            product.ImageUrl = Input.ImageUrl;
            product.CategoryId = Input.CategoryId;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Product updated successfully.";
            return RedirectToPage("Index");
        }
    }
}