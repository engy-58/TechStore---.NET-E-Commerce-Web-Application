using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Data;
using Ecommerce.Models;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class CreateProductModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateProductModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public SelectList Categories { get; set; }

        public class InputModel
        {
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

        public async Task OnGetAsync()
        {
            Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
                return Page();
            }

            var product = new Product
            {
                Name = Input.Name,
                Description = Input.Description,
                Price = Input.Price,
                StockQuantity = Input.StockQuantity,
                SKU = Input.SKU,
                Brand = Input.Brand,
                ImageUrl = Input.ImageUrl,
                CategoryId = Input.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Product created successfully.";
            return RedirectToPage("Index");
        }
    }
}