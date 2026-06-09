using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexusCommerce.Domain.Entities;
using NexusCommerce.Domain.Entities.Identity;
using NexusCommerce.Infrastructure.Data.Context;
using NexusCommerce.Infrastructure.Data.Seeding.Abstract;

namespace NexusCommerce.Infrastructure.Data.Seeding
{
    public class DbInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, NexusCommerceContext dbContext, ILogger<DbInitializer> logger) : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager = roleManager;
        private readonly NexusCommerceContext _dbContext = dbContext;
        private readonly ILogger<DbInitializer> _logger = logger;
        private readonly TimeSpan _operationTimeout = TimeSpan.FromMinutes(5); // Configurable timeout
        public async Task Initialize(CancellationToken cancellationToken = default)
        {
            var seedDate = new DateTime(2026, 6, 6, 10, 30, 0, DateTimeKind.Utc);

            // 1. Create Roles
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
            }
            if (!await _roleManager.RoleExistsAsync("Customer"))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>("Customer"));
            }
            if (!await _roleManager.RoleExistsAsync("Seller"))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>("Seller"));
            }

            // 2. Create Users
            var passwordHash = "AQAAAAIAAYagAAAAEEKyGwLDN+tZyAFPQ74x6xZpmStNgyRKcAgi3Z6z5wmWv4MiPX3+Z0GFmd4Tj638Mw==";

            var user1 = await _userManager.FindByEmailAsync("ahmed@mail.com");
            if (user1 == null)
            {
                user1 = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "ahmed@mail.com",
                    Email = "ahmed@mail.com",
                    FirstName = "Ahmed",
                    LastName = "Ali",
                    IsActive = true,
                    CreatedDate = seedDate,
                    SecurityStamp = "a1b2c3d4-0001-0000-0000-000000000000"
                };
                await _userManager.CreateAsync(user1);
                await _userManager.AddPasswordAsync(user1, "Admin@123"); // Reset password since hash may not work
                await _userManager.AddToRoleAsync(user1, "Customer");
            }

            var user2 = await _userManager.FindByEmailAsync("sara@mail.com");
            if (user2 == null)
            {
                user2 = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "sara@mail.com",
                    Email = "sara@mail.com",
                    FirstName = "Sara",
                    LastName = "Omar",
                    IsActive = true,
                    CreatedDate = seedDate,
                    SecurityStamp = "a1b2c3d4-0002-0000-0000-000000000000"
                };
                await _userManager.CreateAsync(user2);
                await _userManager.AddPasswordAsync(user2, "Admin@123");
                await _userManager.AddToRoleAsync(user2, "Customer");
            }

            var user3 = await _userManager.FindByEmailAsync("store@mail.com");
            if (user3 == null)
            {
                user3 = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "store@mail.com",
                    Email = "store@mail.com",
                    FirstName = "Store",
                    LastName = "Owner",
                    IsActive = true,
                    CreatedDate = seedDate,
                    SecurityStamp = "a1b2c3d4-0003-0000-0000-000000000000"
                };
                await _userManager.CreateAsync(user3);
                await _userManager.AddPasswordAsync(user3, "Admin@123");
                await _userManager.AddToRoleAsync(user3, "Seller");
            }

            // 3. Seed Categories
            if (!_dbContext.Categories.Any())
            {
                await _dbContext.Categories.AddRangeAsync(new[]
                {
                    new Category { Id = Guid.NewGuid(), Name = "Electronics", Description = "Electronic devices and accessories" },
                    new Category { Id = Guid.NewGuid(), Name = "Clothing", Description = "Men and women clothing" },
                    new Category { Id = Guid.NewGuid(), Name = "Home & Garden", Description = "Home and garden supplies" }
                });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // 4. Seed Seller
            if (!_dbContext.Sellers.Any())
            {
                await _dbContext.Sellers.AddAsync(new Seller
                {
                    Id = Guid.NewGuid(),
                    StoreName = "TechZone",
                    Rating = 4.5m,
                    UserId = user3.Id
                });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // 5. Seed Customers
            if (!_dbContext.Customers.Any())
            {
                await _dbContext.Customers.AddRangeAsync(new[]
                {
                    new Customer { Id = Guid.NewGuid(), Address = "123 Nile St, Cairo", UserId = user1.Id },
                    new Customer { Id = Guid.NewGuid(), Address = "456 Delta Rd, Tanta", UserId = user2.Id }
                });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // Get the created customer IDs
            var customer1 = await _dbContext.Customers.FirstOrDefaultAsync(c => c.UserId == user1.Id);
            var customer2 = await _dbContext.Customers.FirstOrDefaultAsync(c => c.UserId == user2.Id);
            var seller = await _dbContext.Sellers.FirstOrDefaultAsync(s => s.UserId == user3.Id);
            var categories = await _dbContext.Categories.ToListAsync();

            // 6. Seed Products
            if (!_dbContext.Products.Any())
            {
                await _dbContext.Products.AddRangeAsync(new[]
                {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Laptop Pro",
                    Description = "15-inch laptop",
                    Price = 1200m,
                    StockQuantity = 50,
                    Rating = 4.7m,
                    Discount = 0,
                    SellerId = seller.Id,
                    CategoryId = categories.First(c => c.Name == "Electronics").Id,
                    CreatedDate = seedDate,
                    ImageUrl = "https://res.cloudinary.com/dtw2jaesz/image/upload/v1772064778/twa9gxhj5vscbmoyy9mq.jpg"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Wireless Mouse",
                    Description = "Ergonomic mouse",
                    Price = 25m,
                    StockQuantity = 200,
                    Rating = 4.3m,
                    Discount = 0,
                    SellerId = seller.Id,
                    CategoryId = categories.First(c => c.Name == "Electronics").Id,
                    CreatedDate = seedDate,
                    ImageUrl = "https://computerguideonline.com/images/mouse-tester.webp"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Cotton T-Shirt",
                    Description = "Comfortable fit",
                    Price = 15m,
                    StockQuantity = 300,
                    Rating = 4.0m,
                    Discount = 0,
                    SellerId = seller.Id,
                    CategoryId = categories.First(c => c.Name == "Clothing").Id,
                    CreatedDate = seedDate,
                    ImageUrl = "https://res.cloudinary.com/dtw2jaesz/image/upload/v1772829317/mmsjwwf6fzzjxepgshrs.avif"
                }
            });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // 7. Seed PaymentMethods
            if (!_dbContext.PaymentMethods.Any())
            {
                await _dbContext.PaymentMethods.AddRangeAsync(new[]
                {
                new PaymentMethod { Id = Guid.NewGuid(), MethodName = "Credit Card" },
                new PaymentMethod { Id = Guid.NewGuid(), MethodName = "Cash on Delivery" },
                new PaymentMethod { Id = Guid.NewGuid(), MethodName = "PayPal" }
            });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var paymentMethod1 = await _dbContext.PaymentMethods.FirstOrDefaultAsync(p => p.MethodName == "Credit Card");
            var laptop = await _dbContext.Products.FirstOrDefaultAsync(p => p.Name == "Laptop Pro");
            var mouse = await _dbContext.Products.FirstOrDefaultAsync(p => p.Name == "Wireless Mouse");
            var tshirt = await _dbContext.Products.FirstOrDefaultAsync(p => p.Name == "Cotton T-Shirt");

            // 8. Seed Order
            if (!_dbContext.Orders.Any())
            {
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer1.Id,
                    PaymentMethodId = paymentMethod1.Id,
                    ShippingAddress = "Cairo",
                    Status = "Pending",
                    TotalAmount = 1225m,
                    CreatedAt = seedDate
                };
                await _dbContext.Orders.AddAsync(order);
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Seed OrderItems
                await _dbContext.OrderItems.AddRangeAsync(new[]
                {
                new OrderItem { Id = Guid.NewGuid(), OrderId = order.Id, ProductId = laptop.Id, Quantity = 1, Price = 1200m },
                new OrderItem { Id = Guid.NewGuid(), OrderId = order.Id, ProductId = mouse.Id, Quantity = 1, Price = 25m }
            });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // 9. Seed Carts
            if (!_dbContext.Carts.Any())
            {
                await _dbContext.Carts.AddRangeAsync(new[]
                {
                new Cart { Id = Guid.NewGuid(), CustomerId = customer1.Id },
                new Cart { Id = Guid.NewGuid(), CustomerId = customer2.Id }
            });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var cart1 = await _dbContext.Carts.FirstOrDefaultAsync(c => c.CustomerId == customer1.Id);

            // 10. Seed CartItems
            if (cart1 != null && !_dbContext.CartItems.Any())
            {
                await _dbContext.CartItems.AddAsync(new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart1.Id,
                    ProductId = tshirt.Id,
                    Quantity = 2
                });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // 11. Seed Wishlists
            if (!_dbContext.Wishlists.Any())
            {
                await _dbContext.Wishlists.AddRangeAsync(new[]
                {
                    new Wishlist { Id = Guid.NewGuid(), CustomerId = customer1.Id },
                    new Wishlist { Id = Guid.NewGuid(), CustomerId = customer2.Id }
                });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var wishlist1 = await _dbContext.Wishlists.FirstOrDefaultAsync(w => w.CustomerId == customer1.Id);

            // 12. Seed WishlistItems
            if (wishlist1 != null && !_dbContext.WishlistItems.Any())
            {
                await _dbContext.WishlistItems.AddAsync(new WishlistItem
                {
                    Id = Guid.NewGuid(),
                    WishlistId = wishlist1.Id,
                    ProductId = laptop.Id
                });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // 13. Seed CustomerPayments
            if (!_dbContext.CustomerPayments.Any())
            {
                await _dbContext.CustomerPayments.AddAsync(new CustomerPayment
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer1.Id,
                    PaymentMethodId = paymentMethod1.Id
                });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}