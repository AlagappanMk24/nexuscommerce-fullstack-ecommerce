using NexusCommerce.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexusCommerce.Domain.Entities
{
    public class Wishlist : BaseEntity
    {
        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    }
}