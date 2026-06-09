using NexusCommerce.Domain.Common;

namespace NexusCommerce.Domain.Entities
{
    public class PaymentMethod : BaseEntity
    {
        public string MethodName { get; set; }
        public ICollection<CustomerPayment> CustomerPayments { get; set; } = new List<CustomerPayment>();

        public ICollection<Order> Orders = [];
    }
}