using NexusCommerce.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexusCommerce.Domain.Entities
{
    public class WishlistItem : BaseEntity
    {
        [ForeignKey("Wishlist")]
        public Guid WishlistId { get; set; }

        [ForeignKey("Product")]
        public Guid ProductId { get; set; }
        public Wishlist Wishlist { get; set; }
        public Product Product { get; set; }
    }
}