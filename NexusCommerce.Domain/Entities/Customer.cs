using NexusCommerce.Domain.Common;
using NexusCommerce.Domain.Entities.Identity;

namespace NexusCommerce.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string? Address { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<Order> Orders { get; set; }
        public Cart Cart { get; set; }
        public Wishlist Wishlist { get; set; }
    }
}