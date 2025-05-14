using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ecommerce.Data;
using Ecommerce.Models;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class EditCategoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditCategoryModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public int Id { get; set; }

            [Required]
            [StringLength(50)]
            public string Name { get; set; }

            [Required]
            [StringLength(200)]
            public string Description { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToPage("Index");
            }

            Input = new InputModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var category = await _context.Categories.FindAsync(Input.Id);
            if (category == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToPage("Index");
            }

            category.Name = Input.Name;
            category.Description = Input.Description;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Category updated successfully.";
            return RedirectToPage("Index");
        }
    }
}