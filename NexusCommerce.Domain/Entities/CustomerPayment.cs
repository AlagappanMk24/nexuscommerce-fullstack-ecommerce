using NexusCommerce.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexusCommerce.Domain.Entities
{
    public class CustomerPayment : BaseEntity
    {
        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }

        [ForeignKey("PaymentMethod")]
        public Guid PaymentMethodId { get; set; }
        public Customer Customer { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}