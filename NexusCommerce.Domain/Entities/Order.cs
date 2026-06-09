using NexusCommerce.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexusCommerce.Domain.Entities
{
    public class Order : BaseEntity
    {
        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }

        [ForeignKey("PaymentMethod")]
        public Guid PaymentMethodId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public Customer Customer { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}