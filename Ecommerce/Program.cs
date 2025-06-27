using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedDataAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

async Task SeedDataAsync(IServiceProvider services)
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    // Apply migrations
    await context.Database.MigrateAsync();
    logger.LogInformation("Database migrations applied.");

    // Seed roles
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
        logger.LogInformation("Admin role created.");
    }

    // Seed admin user
    var adminEmail = "admin@example.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Admin User",
            ShippingAddress = "123 Admin St",
            City = " AOL Admin City",
            PostalCode = "12345",
            Country = "Adminland",
            PhoneNumber = "123-456-7890"
        };
        var result = await userManager.CreateAsync(adminUser, builder.Configuration["DefaultPasswords:AdminPassword"] ?? "TempAdmin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("Admin user created and assigned to Admin role.");
        }
        else
        {
            logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    // Seed regular user
    var userEmail = "john@example.com";
    var user = await userManager.FindByEmailAsync(userEmail);
    if (user == null)
    {
        user = new User
        {
            UserName = userEmail,
            Email = userEmail,
            FullName = "John Doe",
            ShippingAddress = "456 User Rd",
            City = "User City",
            PostalCode = "67890",
            Country = "Userland",
            PhoneNumber = "987-654-3210"
        };
        var result = await userManager.CreateAsync(user, builder.Configuration["DefaultPasswords:UserPassword"] ?? "TempUser123!");
        if (result.Succeeded)
        {
            logger.LogInformation("Regular user created.");
        }
        else
        {
            logger.LogError("Failed to create regular user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    // Seed categories
    if (!context.Categories.Any())
    {
        var categories = new[]
        {
            new Category { Name = "Electronics" },
            new Category { Name = "Clothing" },
            new Category { Name = "Books" }
        };
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
        logger.LogInformation("Categories seeded.");
    }

    // Seed products
    if (!context.Products.Any())
    {
        var electronicsCategory = await context.Categories.FirstAsync(c => c.Name == "Electronics");
        var clothingCategory = await context.Categories.FirstAsync(c => c.Name == "Clothing");
        var booksCategory = await context.Categories.FirstAsync(c => c.Name == "Books");

        var products = new[]
        {
            new Product
            {
                Name = "Smartphone",
                Description = "Latest model with 5G",
                Price = 699.99m,
                StockQuantity = 50,
                CategoryId = electronicsCategory.Id,
                Brand = "TechBrand",
                SKU = "SM001",
                ImageUrl = "/images/smartphone.jpg"
            },
            new Product
            {
                Name = "Laptop",
                Description = "High-performance laptop with Intel i7 processor",
                Price = 999.99m,
                StockQuantity = 30,
                CategoryId = electronicsCategory.Id,
                Brand = "TechBrand",
                SKU = "LT001",
                ImageUrl = "/images/laptop.jpg"
            },
            new Product
            {
                Name = "Smartwatch",
                Description = "Fitness smartwatch with heart rate monitor",
                Price = 199.99m,
                StockQuantity = 40,
                CategoryId = electronicsCategory.Id,
                Brand = "TechBrand",
                SKU = "SW001",
                ImageUrl = "/images/smartwatch.jpg"
            },
            new Product
            {
                Name = "Headphones",
                Description = "Wireless noise-cancelling headphones",
                Price = 149.99m,
                StockQuantity = 60,
                CategoryId = electronicsCategory.Id,
                Brand = "AudioBrand",
                SKU = "HP001",
                ImageUrl = "/images/headphones.jpg"
            },
            new Product
            {
                Name = "T-Shirt",
                Description = "Comfortable cotton t-shirt",
                Price = 19.99m,
                StockQuantity = 100,
                CategoryId = clothingCategory.Id,
                Brand = "FashionCo",
                SKU = "TS001",
                ImageUrl = "/images/tshirt.jpg"
            },
            new Product
            {
                Name = "Jeans",
                Description = "Stylish denim jeans",
                Price = 49.99m,
                StockQuantity = 50,
                CategoryId = clothingCategory.Id,
                Brand = "FashionCo",
                SKU = "JN001",
                ImageUrl = "/images/jeans.jpg"
            },
            new Product
            {
                Name = "Jacket",
                Description = "Warm winter jacket",
                Price = 79.99m,
                StockQuantity = 40,
                CategoryId = clothingCategory.Id,
                Brand = "FashionCo",
                SKU = "JK001",
                ImageUrl = "/images/jacket.jpg"
            },
            new Product
            {
                Name = "Novel",
                Description = "An exciting fictional novel",
                Price = 9.99m,
                StockQuantity = 200,
                CategoryId = booksCategory.Id,
                Brand = "BookBrand",
                SKU = "BN001",
                ImageUrl = "/images/novel.jpg"
            },
            new Product
            {
                Name = "Textbook",
                Description = "Comprehensive guide for advanced studies",
                Price = 39.99m,
                StockQuantity = 50,
                CategoryId = booksCategory.Id,
                Brand = "BookBrand",
                SKU = "TX001",
                ImageUrl = "/images/textbook.jpg"
            },
            new Product
            {
                Name = "Cookbook",
                Description = "Collection of delicious recipes",
                Price = 19.99m,
                StockQuantity = 80,
                CategoryId = booksCategory.Id,
                Brand = "BookBrand",
                SKU = "CB001",
                ImageUrl = "/images/cookbook.jpg"
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
        logger.LogInformation("Products seeded with images.");
    }

    // Seed cart and order
    if (!context.Orders.Any())
    {
        var userId = (await userManager.FindByEmailAsync(userEmail))?.Id;
        if (userId == null)
        {
            logger.LogError("User {UserEmail} not found for seeding cart and order.", userEmail);
            return;
        }

        var cart = await context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
        {
            cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
            var smartphone = await context.Products.FirstAsync(p => p.Name == "Smartphone");
            cart.CartItems.Add(new CartItem
            {
                ProductId = smartphone.Id,
                Quantity = 1
            });
            context.Carts.Add(cart);
            await context.SaveChangesAsync();
            logger.LogInformation("Cart seeded for user {UserId}.", userId);
        }

        var products = await context.Products
            .Where(p => cart.CartItems.Select(ci => ci.ProductId).Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p);

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * products[ci.ProductId].Price),
            Status = OrderStatus.Pending,
            PaymentMethod = "CreditCard",
            ShippingAddress = "456 User Rd, User City, 67890, Userland",
            OrderItems = cart.CartItems.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                ProductName = products[ci.ProductId].Name,
                Price = products[ci.ProductId].Price,
                Quantity = ci.Quantity
            }).ToList()
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        logger.LogInformation("Order seeded for user {UserId}.", userId);
    }
}