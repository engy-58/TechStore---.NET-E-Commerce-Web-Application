# TechStore - .NET E-Commerce Web Application

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=.net)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-512BD4?style=flat&logo=.net)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core%209.0-512BD4?style=flat)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=flat&logo=microsoft-sql-server)

A modern, full-featured e-commerce web application built with ASP.NET Core 9.0, featuring a clean architecture, user authentication, product management, shopping cart functionality, and an admin dashboard.

## 🚀 Features

### Customer Features
- **User Authentication & Authorization** - Secure registration, login, and role-based access
- **Product Browsing** - Browse products by categories with detailed product pages
- **Shopping Cart** - Add/remove items, modify quantities, and session management
- **Order Management** - Place orders, view order history, and order confirmation
- **User Profiles** - Manage personal information and view order history
- **Product Reviews** - Read and write product reviews

### Admin Features
- **Admin Dashboard** - Comprehensive overview of store metrics and activities
- **Product Management** - Create, edit, and delete products
- **Category Management** - Organize products into categories
- **Order Management** - View and manage customer orders
- **User Management** - Handle user accounts and roles

### Technical Features
- **Responsive Design** - Mobile-friendly interface using Bootstrap
- **Entity Framework Core** - Code-first database approach with migrations
- **Identity Framework** - Robust user authentication and authorization
- **Session Management** - Secure cart and user session handling
- **Email Support** - Order confirmations and notifications via MailKit
- **PDF Generation** - Invoice and receipt generation using IronPDF

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 9.0
- **Database**: SQL Server with Entity Framework Core 9.0
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Razor Pages, MVC, Bootstrap, HTML5, CSS3, JavaScript
- **Email**: MailKit for email notifications
- **PDF Generation**: IronPDF for document generation
- **Security**: BCrypt.Net for password hashing

## 📋 Prerequisites

Before running this application, make sure you have the following installed:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or Full)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/TechStore---.NET-E-Commerce-Web-App.git
cd TechStore---.NET-E-Commerce-Web-App
```

### 2. Configure Database Connection
Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EcommerceDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Install Dependencies
```bash
cd Ecommerce
dotnet restore
```

### 4. Apply Database Migrations
```bash
dotnet ef database update
```

### 5. Run the Application
```bash
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`.

## 📁 Project Structure

```
Ecommerce/
├── Areas/
│   ├── Admin/                 # Admin area with Razor Pages
│   │   └── Pages/
│   │       ├── Dashboard.cshtml
│   │       ├── Orders/
│   │       └── Products/
│   └── Identity/              # Identity UI pages
├── Controllers/               # MVC Controllers
│   ├── CartController.cs
│   ├── HomeController.cs
│   ├── OrderController.cs
│   ├── ProductController.cs
│   ├── ProfileController.cs
│   └── ReviewController.cs
├── Data/
│   └── ApplicationDbContext.cs # Entity Framework DbContext
├── Models/                    # Data models
│   ├── Cart.cs
│   ├── Category.cs
│   ├── Order.cs
│   ├── Product.cs
│   ├── Review.cs
│   └── User.cs
├── ViewModels/               # View models for MVC
├── Views/                    # Razor views
│   ├── Cart/
│   ├── Home/
│   ├── Order/
│   ├── Product/
│   ├── Profile/
│   └── Shared/
└── wwwroot/                  # Static files
    └── images/               # Product images
```

## 🔧 Configuration

### Database Configuration
The application uses SQL Server as the default database. You can modify the connection string in `appsettings.json` to use:
- SQL Server LocalDB (default)
- SQL Server Express
- Full SQL Server instance
- SQLite (change provider in `Program.cs`)

### Authentication Settings
User authentication is configured with the following password requirements:
- Minimum 8 characters
- Requires uppercase letter
- Requires lowercase letter  
- Requires digit
- No special characters required

### Session Configuration
- Session timeout: 30 minutes
- Cookie expiration: 14 days (sliding)
- HTTP-only cookies for security

## 🎯 Usage

### For Customers
1. **Register/Login**: Create an account or sign in
2. **Browse Products**: Explore products by categories
3. **Add to Cart**: Select products and add them to your shopping cart
4. **Checkout**: Complete your purchase with order confirmation
5. **Manage Profile**: Update personal information and view order history

### For Administrators
1. **Access Admin Panel**: Login with admin credentials
2. **Manage Products**: Add, edit, or remove products
3. **Manage Categories**: Organize product categories
4. **View Orders**: Monitor and manage customer orders
5. **Dashboard**: View sales metrics and store analytics

## 🔐 Security Features

- **Password Hashing**: BCrypt.Net for secure password storage
- **SQL Injection Protection**: Entity Framework parameterized queries
- **XSS Protection**: Razor page encoding and validation
- **CSRF Protection**: Anti-forgery tokens
- **Role-based Authorization**: Admin and user role separation
- **Secure Cookies**: HTTP-only and secure cookie settings

## 🧪 Testing

To run the application in development mode:
```bash
dotnet run --environment Development
```

## 📝 API Documentation

The application follows RESTful conventions for its controller actions:

### Product Controller
- `GET /Product` - List all products
- `GET /Product/Details/{id}` - Get product details
- `GET /Product/List` - Product listing with filtering

### Cart Controller
- `GET /Cart` - View cart contents
- `POST /Cart/Add` - Add item to cart
- `POST /Cart/Update` - Update cart quantities
- `POST /Cart/Remove` - Remove item from cart

### Order Controller
- `GET /Order/History` - View order history
- `GET /Order/Checkout` - Checkout page
- `POST /Order/PlaceOrder` - Place new order

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Zahraa Selim** 
- **Engy Ahmed**
- **Menna Noseer**
- **Esraa Magdy** 
---

**Happy Shopping! 🛍️**
