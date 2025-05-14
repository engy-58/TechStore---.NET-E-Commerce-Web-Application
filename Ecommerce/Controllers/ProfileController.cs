using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(ApplicationDbContext context, UserManager<User> userManager, ILogger<ProfileController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError("User {UserId} not found.", userId);
                return NotFound();
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var model = new ProfileIndexViewModel
            {
                Profile = new ProfileViewModel
                {
                    FullName = user.FullName,
                    ShippingAddress = user.ShippingAddress,
                    City = user.City,
                    PostalCode = user.PostalCode,
                    Country = user.Country,
                    PhoneNumber = user.PhoneNumber
                },
                Orders = orders.Select(o => new OrderConfirmationViewModel
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status.ToString(),
                    TotalAmount = o.TotalAmount,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        UnitPrice = oi.Price,
                        Quantity = oi.Quantity,
                        ImageUrl = string.IsNullOrEmpty(oi.Product.ImageUrl) ? "/images/default.jpg" : oi.Product.ImageUrl
                    }).ToList()
                }).ToList()
            };

            _logger.LogInformation("User {UserId} viewed profile.", userId);
            return View(model);
        }

        public async Task<IActionResult> Edit(string returnUrl)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError("User {UserId} not found.", userId);
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                ShippingAddress = user.ShippingAddress,
                City = user.City,
                PostalCode = user.PostalCode,
                Country = user.Country,
                PhoneNumber = user.PhoneNumber
            };

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model, string returnUrl)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError("User {UserId} not found.", userId);
                return NotFound();
            }

            // Log form data for debugging
            _logger.LogInformation("Form data received: returnUrl={ReturnUrl}, FullName={FullName}", returnUrl ?? "null", model.FullName);

            // Clear ModelState for returnUrl and validate only ProfileViewModel
            ModelState.Clear();
            TryValidateModel(model);

            if (ModelState.IsValid)
            {
                user.FullName = model.FullName;
                user.ShippingAddress = model.ShippingAddress;
                user.City = model.City;
                user.PostalCode = model.PostalCode;
                user.Country = model.Country;
                user.PhoneNumber = model.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User profile updated for user {UserId}.", userId);
                    TempData["Success"] = "Profile updated successfully.";

                    // Redirect to Profile/Index by default, unless returnUrl is valid and points to checkout
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl == "/Order/Checkout")
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    _logger.LogWarning("Profile update error for user {UserId}: {Error}", userId, error.Description);
                }
            }
            else
            {
                // Log all ModelState errors for debugging
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                _logger.LogWarning("Invalid model state for user profile update for user {UserId}. Errors: {Errors}", userId, string.Join(", ", errors));
            }

            // Log ModelState keys for debugging
            var modelStateKeys = ModelState.Keys.ToList();
            _logger.LogInformation("ModelState keys: {Keys}", string.Join(", ", modelStateKeys));

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }
    }
}