using NexusCommerce.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexusCommerce.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Discount { get; set; } = 0;
        public int StockQuantity { get; set; }
        public decimal? Rating { get; set; }

        [ForeignKey(nameof(Seller))]
        public Guid SellerId { get; set; }

        [ForeignKey(nameof(Category))]
        public Guid CategoryId { get; set; }
        public int ReviewCount { get; set; } = 0;
        public Category? Category { get; set; }
        public Seller? Seller { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    }
}