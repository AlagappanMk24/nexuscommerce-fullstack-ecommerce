using NexusCommerce.Domain.Common;

namespace NexusCommerce.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}