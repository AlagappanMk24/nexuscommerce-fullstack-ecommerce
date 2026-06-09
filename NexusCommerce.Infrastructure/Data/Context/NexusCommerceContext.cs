using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NexusCommerce.Domain.Entities;
using NexusCommerce.Domain.Entities.Identity;

namespace NexusCommerce.Infrastructure.Data.Context
{
    public class NexusCommerceContext(DbContextOptions<NexusCommerceContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /// <summary>
            /// Applies all configurations defined in the Assembly (IEntityTypeConfiguration)
            /// </summary>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NexusCommerceContext).Assembly);
        }
        public virtual DbSet<Customer> Customers => Set<Customer>();
        public virtual DbSet<Seller> Sellers => Set<Seller>();
        public virtual DbSet<Product> Products => Set<Product>();
        public virtual DbSet<Category> Categories => Set<Category>();
        public virtual DbSet<Order> Orders => Set<Order>();
        public virtual DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public virtual DbSet<Cart> Carts => Set<Cart>();
        public virtual DbSet<CartItem> CartItems => Set<CartItem>();
        public virtual DbSet<Wishlist> Wishlists => Set<Wishlist>();
        public virtual DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
        public virtual DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
        public virtual DbSet<CustomerPayment> CustomerPayments => Set<CustomerPayment>();
        public virtual DbSet<Review> Reviews => Set<Review>();
    }
}