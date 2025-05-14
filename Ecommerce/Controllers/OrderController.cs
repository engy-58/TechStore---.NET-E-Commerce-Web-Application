using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ApplicationDbContext context, UserManager<User> userManager, ILogger<OrderController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                _logger.LogWarning("Cart is empty for user {UserId}.", userId);
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            if (string.IsNullOrEmpty(user.ShippingAddress) || string.IsNullOrEmpty(user.FullName))
            {
                _logger.LogWarning("Incomplete user profile for user {UserId}.", userId);
                TempData["Error"] = "Please complete your profile before checkout.";
                return RedirectToAction("Edit", "Profile", new { returnUrl = Url.Action("Checkout", "Order") });
            }

            var model = new CheckoutViewModel
            {
                PaymentMethods = new List<string> { "CreditCard", "PayPal" },
                Profile = new ProfileViewModel
                {
                    FullName = user.FullName,
                    ShippingAddress = user.ShippingAddress,
                    City = user.City,
                    PostalCode = user.PostalCode,
                    Country = user.Country,
                    PhoneNumber = user.PhoneNumber
                },
                CartItems = cart.CartItems.Select(ci => new CartItemViewModel
                {
                    CartItemId = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Price = ci.Product.Price,
                    Quantity = ci.Quantity,
                    ImageUrl = string.IsNullOrEmpty(ci.Product.ImageUrl) ? "/images/default.jpg" : ci.Product.ImageUrl
                }).ToList(),
                TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price)
            };

            _logger.LogInformation("Checkout view loaded for user {UserId}.", userId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                _logger.LogWarning("Cart is empty during checkout for user {UserId}.", userId);
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                model.PaymentMethods = new List<string> { "CreditCard", "PayPal" };
                model.Profile = new ProfileViewModel
                {
                    FullName = user.FullName,
                    ShippingAddress = user.ShippingAddress,
                    City = user.City,
                    PostalCode = user.PostalCode,
                    Country = user.Country,
                    PhoneNumber = user.PhoneNumber
                };
                model.CartItems = cart.CartItems.Select(ci => new CartItemViewModel
                {
                    CartItemId = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Price = ci.Product.Price,
                    Quantity = ci.Quantity,
                    ImageUrl = string.IsNullOrEmpty(ci.Product.ImageUrl) ? "/images/default.jpg" : ci.Product.ImageUrl
                }).ToList();
                model.TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
                _logger.LogWarning("Invalid checkout model state for user {UserId}.", userId);
                return View(model);
            }

            // Validate stock
            foreach (var item in cart.CartItems)
            {
                if (item.Quantity > item.Product.StockQuantity)
                {
                    _logger.LogWarning("Insufficient stock for product {ProductId} during checkout for user {UserId}.", item.ProductId, userId);
                    TempData["Error"] = $"Insufficient stock for {item.Product.Name}.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price),
                Status = OrderStatus.Pending,
                PaymentMethod = model.PaymentMethod,
                ShippingAddress = $"{user.ShippingAddress}, {user.City}, {user.PostalCode}, {user.Country}",
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Price = ci.Product.Price,
                    Quantity = ci.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);

            // Update stock
            foreach (var item in cart.CartItems)
            {
                item.Product.StockQuantity -= item.Quantity;
            }

            // Clear cart
            _context.CartItems.RemoveRange(cart.CartItems);
            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Order {OrderId} created for user {UserId}.", order.Id, userId);
            TempData["Success"] = "Order placed successfully.";
            return RedirectToAction("Confirmation", new { orderId = order.Id });
        }

        public async Task<IActionResult> Confirmation(int orderId)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for user {UserId}.", orderId, userId);
                return NotFound();
            }

            var model = new OrderConfirmationViewModel
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    UnitPrice = oi.Price,
                    Quantity = oi.Quantity,
                    ImageUrl = string.IsNullOrEmpty(oi.Product.ImageUrl) ? "/images/default.jpg" : oi.Product.ImageUrl
                }).ToList()
            };

            _logger.LogInformation("Order confirmation viewed for order {OrderId} by user {UserId}.", orderId, userId);
            return View(model);
        }

        public async Task<IActionResult> History()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var model = orders.Select(o => new OrderConfirmationViewModel
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                PaymentMethod = o.PaymentMethod,
                OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    UnitPrice = oi.Price,
                    Quantity = oi.Quantity,
                    ImageUrl = string.IsNullOrEmpty(oi.Product.ImageUrl) ? "/images/default.jpg" : oi.Product.ImageUrl
                }).ToList()
            }).ToList();

            _logger.LogInformation("Order history viewed by user {UserId}.", userId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int orderId)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for user {UserId}.", orderId, userId);
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Index", "Profile");
            }

            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogWarning("Cannot cancel order {OrderId} with status {Status} for user {UserId}.", orderId, order.Status, userId);
                TempData["Error"] = "Only pending orders can be canceled.";
                return RedirectToAction("Index", "Profile");
            }

            order.Status = OrderStatus.Canceled;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Order {OrderId} canceled by user {UserId}.", orderId, userId);
            TempData["Success"] = "Order canceled successfully.";
            return RedirectToAction("Index", "Profile");
        }
    }
}