using NexusCommerce.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexusCommerce.Domain.Entities
{
    public class Cart : BaseEntity
    {
        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}