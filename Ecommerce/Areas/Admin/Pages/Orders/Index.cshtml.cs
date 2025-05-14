using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Data;
using Ecommerce.Models;

namespace Ecommerce.Areas.Admin.Pages.Orders
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Order> Orders { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }
        [BindProperty(SupportsGet = true)]
        public OrderStatus? StatusFilter { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.User)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(SearchQuery))
                {
                    query = query.Where(o => o.Id.ToString().Contains(SearchQuery) || o.User.FullName.Contains(SearchQuery));
                }

                if (StatusFilter.HasValue)
                {
                    query = query.Where(o => o.Status == StatusFilter.Value);
                }

                Orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                // Log the error
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, OrderStatus status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToPage();
                }

                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Order status updated successfully.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToPage();
            }
        }
    }
}