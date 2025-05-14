using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ecommerce.Data;
using Ecommerce.Models;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class CreateCategoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateCategoryModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(50)]
            public string Name { get; set; }

            [Required]
            [StringLength(200)]
            public string Description { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var category = new Category
            {
                Name = Input.Name,
                Description = Input.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category created successfully.";
            return RedirectToPage("Index");
        }
    }
}