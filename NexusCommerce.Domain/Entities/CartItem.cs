using NexusCommerce.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexusCommerce.Domain.Entities
{
    public class CartItem : BaseEntity
    {
        [ForeignKey("Cart")]
        public Guid CartId { get; set; }

        [ForeignKey("Product")]
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public Cart Cart { get; set; }
        public Product Product { get; set; }
    }
}