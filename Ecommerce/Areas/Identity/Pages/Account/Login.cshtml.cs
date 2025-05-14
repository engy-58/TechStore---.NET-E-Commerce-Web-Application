using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Ecommerce.Data;
using Ecommerce.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _context;

        public LoginModel(SignInManager<User> signInManager, UserManager<User> userManager, ILogger<LoginModel> logger, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in with returnUrl: {ReturnUrl}.", ReturnUrl);
                    var user = await _userManager.FindByEmailAsync(Input.Email);

                    // Merge guest cart if the returnUrl is for the cart
                    if (Url.IsLocalUrl(ReturnUrl) && ReturnUrl == "/Cart/Index")
                    {
                        await MergeGuestCart(user.Id);
                        _logger.LogInformation("Merged guest cart for user {Email}.", Input.Email);
                        return RedirectToAction("Index", "Cart"); // Redirect to CartController's Index action
                    }

                    // If user is an admin, redirect to the Admin Dashboard
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToPage("/Dashboard", new { area = "Admin" });
                    }

                    // Otherwise, do a local redirect based on the ReturnUrl
                    return LocalRedirect(ReturnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            return Page();
        }

        private async Task MergeGuestCart(string userId)
        {
            // Get guest cart from session
            var guestCartJson = HttpContext.Session.GetString("GuestCart");
            if (string.IsNullOrEmpty(guestCartJson))
                return;

            var guestCart = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CartItemViewModel>>(guestCartJson);
            if (!guestCart.Any())
                return;

            // Get or create the user's cart in the database
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
                _context.Carts.Add(cart);
            }

            // Merge guest cart items
            foreach (var guestItem in guestCart)
            {
                var product = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == guestItem.ProductId);

                if (product == null || guestItem.Quantity > product.StockQuantity)
                {
                    _logger.LogWarning("Product {ProductId} not found or quantity {Quantity} exceeds stock {StockQuantity}.", guestItem.ProductId, guestItem.Quantity, product?.StockQuantity);
                    continue;
                }

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == guestItem.ProductId);
                if (cartItem != null)
                {
                    cartItem.Quantity += guestItem.Quantity;
                    if (cartItem.Quantity > product.StockQuantity)
                    {
                        cartItem.Quantity = product.StockQuantity; // Cap at stock quantity
                        _logger.LogWarning("Capped quantity to stock {StockQuantity} for product {ProductId}.", product.StockQuantity, guestItem.ProductId);
                    }
                }
                else
                {
                    cart.CartItems.Add(new CartItem
                    {
                        ProductId = guestItem.ProductId,
                        Quantity = guestItem.Quantity
                    });
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Clear the guest cart from session
            HttpContext.Session.Remove("GuestCart");
        }
    }
}