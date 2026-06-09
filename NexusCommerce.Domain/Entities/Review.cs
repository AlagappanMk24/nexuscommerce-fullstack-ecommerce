using NexusCommerce.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexusCommerce.Domain.Entities
{
    public class Review : BaseEntity
    {
        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }

        [ForeignKey("Product")]
        public Guid ProductId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; } = string.Empty;
        public Product Product { get; set; }
        public Customer Customer { get; set; }
    }
}