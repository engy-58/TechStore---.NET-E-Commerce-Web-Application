using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext context, UserManager<User> userManager, ILogger<CartController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            List<CartItemViewModel> cartItems = new List<CartItemViewModel>();
            decimal total = 0;

            if (string.IsNullOrEmpty(userId))
            {
                // For guest users, load cart from session (if any).
                cartItems = GetGuestCart();
                // Assign ProductId as CartItemId for guest users to align with UpdateQuantity/Remove
                foreach (var item in cartItems)
                {
                    item.CartItemId = item.ProductId; // Use ProductId as a temporary CartItemId
                }
                total = cartItems.Sum(ci => ci.Quantity * ci.Price);
            }
            else
            {
                // For authenticated users, load cart from the database.
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                cartItems = cart?.CartItems.Select(ci => new CartItemViewModel
                {
                    CartItemId = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Price = ci.Product.Price,
                    Quantity = ci.Quantity,
                    ImageUrl = string.IsNullOrEmpty(ci.Product.ImageUrl) ? "/images/default.jpg" : ci.Product.ImageUrl
                }).ToList() ?? new List<CartItemViewModel>();

                total = cart?.CartItems.Sum(ci => ci.Quantity * ci.Product.Price) ?? 0;
            }

            var model = new CartViewModel
            {
                CartItems = cartItems,
                Total = total
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (quantity < 1)
            {
                _logger.LogWarning("Invalid quantity {Quantity} for product {ProductId}.", quantity, productId);
                TempData["Error"] = "Quantity must be at least 1.";
                return RedirectToAction("Index", "Home");
            }

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found.", productId);
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index", "Home");
            }

            if (quantity > product.StockQuantity)
            {
                _logger.LogWarning("Requested quantity {Quantity} exceeds stock {StockQuantity} for product {ProductId}.", quantity, product.StockQuantity, productId);
                TempData["Error"] = "Requested quantity exceeds available stock.";
                return RedirectToAction("Index", "Home");
            }

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                // For guest users, store cart in session
                var guestCart = GetGuestCart();
                var existingItem = guestCart.FirstOrDefault(ci => ci.ProductId == productId);
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    guestCart.Add(new CartItemViewModel
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        ProductName = product.Name,
                        Price = product.Price,
                        ImageUrl = string.IsNullOrEmpty(product.ImageUrl) ? "/images/default.jpg" : product.ImageUrl
                    });
                }

                SaveGuestCart(guestCart);
                TempData["Success"] = "Item added to cart. Please log in to proceed.";
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = "/Cart/Index" });
            }
            else
            {
                // For logged-in users, add to the database
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                if (cartItem != null)
                {
                    cartItem.Quantity += quantity;
                    if (cartItem.Quantity > product.StockQuantity)
                    {
                        _logger.LogWarning("Updated quantity {Quantity} exceeds stock {StockQuantity} for product {ProductId}.", cartItem.Quantity, product.StockQuantity, productId);
                        TempData["Error"] = "Updated quantity exceeds available stock.";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    cart.CartItems.Add(new CartItem
                    {
                        ProductId = productId,
                        Quantity = quantity
                    });
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Item added to cart.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                // Handle guest user cart in session
                var guestCart = GetGuestCart();
                var guestItem = guestCart.FirstOrDefault(ci => ci.ProductId == cartItemId); // Use ProductId for guest users
                if (guestItem == null)
                {
                    _logger.LogWarning("Cart item {CartItemId} not found in guest cart.", cartItemId);
                    TempData["Error"] = "Cart item not found.";
                    return RedirectToAction("Index");
                }

                var product = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == guestItem.ProductId);

                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found for guest cart item.", guestItem.ProductId);
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                if (quantity < 1)
                {
                    guestCart.Remove(guestItem);
                    SaveGuestCart(guestCart);
                    _logger.LogInformation("Guest cart item {CartItemId} removed.", cartItemId);
                    TempData["Success"] = "Item removed from cart.";
                    return RedirectToAction("Index");
                }

                if (quantity > product.StockQuantity)
                {
                    _logger.LogWarning("Requested quantity {Quantity} exceeds stock {StockQuantity} for product {ProductId}.", quantity, product.StockQuantity, guestItem.ProductId);
                    TempData["Error"] = "Requested quantity exceeds available stock.";
                    return RedirectToAction("Index");
                }

                guestItem.Quantity = quantity;
                SaveGuestCart(guestCart);
                TempData["Success"] = "Cart updated successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                // Handle authenticated user cart in database
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .Include(ci => ci.Product)
                    .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

                if (cartItem == null)
                {
                    _logger.LogWarning("Cart item {CartItemId} not found for user {UserId}.", cartItemId, userId);
                    TempData["Error"] = "Cart item not found.";
                    return RedirectToAction("Index");
                }

                if (quantity < 1)
                {
                    _context.CartItems.Remove(cartItem);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Cart item {CartItemId} removed for user {UserId}.", cartItemId, userId);
                    TempData["Success"] = "Item removed from cart.";
                    return RedirectToAction("Index");
                }

                if (quantity > cartItem.Product.StockQuantity)
                {
                    _logger.LogWarning("Requested quantity {Quantity} exceeds stock {StockQuantity} for cart item {CartItemId}.", quantity, cartItem.Product.StockQuantity, cartItemId);
                    TempData["Error"] = "Requested quantity exceeds available stock.";
                    return RedirectToAction("Index");
                }

                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cart item {CartItemId} quantity updated to {Quantity} for user {UserId}.", cartItemId, quantity, userId);
                TempData["Success"] = "Cart updated successfully.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                // Handle guest user cart in session
                var guestCart = GetGuestCart();
                var guestItem = guestCart.FirstOrDefault(ci => ci.ProductId == cartItemId); // Use ProductId for guest users
                if (guestItem == null)
                {
                    _logger.LogWarning("Cart item {CartItemId} not found in guest cart.", cartItemId);
                    TempData["Error"] = "Cart item not found.";
                    return RedirectToAction("Index");
                }

                guestCart.Remove(guestItem);
                SaveGuestCart(guestCart);
                TempData["Success"] = "Item removed from cart.";
                return RedirectToAction("Index");
            }
            else
            {
                // Handle authenticated user cart in database
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

                if (cartItem == null)
                {
                    _logger.LogWarning("Cart item {CartItemId} not found for user {UserId}.", cartItemId, userId);
                    TempData["Error"] = "Cart item not found.";
                    return RedirectToAction("Index");
                }

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cart item {CartItemId} removed for user {UserId}.", cartItemId, userId);
                TempData["Success"] = "Item removed from cart.";
                return RedirectToAction("Index");
            }
        }

        private List<CartItemViewModel> GetGuestCart()
        {
            var cart = HttpContext.Session.GetString("GuestCart");
            if (string.IsNullOrEmpty(cart))
                return new List<CartItemViewModel>();

            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<CartItemViewModel>>(cart);
        }

        private void SaveGuestCart(List<CartItemViewModel> cart)
        {
            var cartJson = Newtonsoft.Json.JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("GuestCart", cartJson);
        }
    }
}