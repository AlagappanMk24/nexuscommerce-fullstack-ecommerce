using System.ComponentModel.DataAnnotations;

namespace NexusCommerce.Domain.Common
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; } = false; // Default to not deleted
        public string? CreatedBy { get; set; } // Nullable string
        public string? UpdatedBy { get; set; } // Nullable string
    }
}