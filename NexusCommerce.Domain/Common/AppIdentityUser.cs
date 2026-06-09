using Microsoft.AspNetCore.Identity;

namespace NexusCommerce.Domain.Common
{
    // Inherit from IdentityUser<Guid> so the base class uses Guid for Id
    public class AppIdentityUser : IdentityUser<Guid>
    {
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}