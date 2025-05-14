using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Data;
using Ecommerce.Models;

namespace Ecommerce.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int LowStockProducts { get; set; }
        public List<Order> RecentOrders { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                TotalOrders = await _context.Orders.CountAsync();
                TotalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);
                LowStockProducts = await _context.Products.CountAsync(p => p.StockQuantity < 10);
                RecentOrders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                // Log the error (e.g., to console or a logging service)
                Console.WriteLine(ex.ToString());
            }
        }
    }
}