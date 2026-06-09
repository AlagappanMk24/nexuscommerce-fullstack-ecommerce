using NexusCommerce.Domain.Common;
using NexusCommerce.Domain.Entities.Identity;

namespace NexusCommerce.Domain.Entities
{
    public class Seller : BaseEntity
    {
        public string StoreName { get; set; } = null!;
        public decimal? Rating { get; set; }
        public Guid UserId { get; set; }
        public ICollection<Product> Products { get; set; } = new HashSet<Product>();
        public ApplicationUser? User { get; set; }
    }
}