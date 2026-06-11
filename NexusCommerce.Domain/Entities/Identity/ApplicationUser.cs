using NexusCommerce.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace NexusCommerce.Domain.Entities.Identity
{
    public class ApplicationUser : AppIdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // Two-Factor Authentication Properties
        public string? TwoFactorCode { get; set; }
        public DateTime? TwoFactorExpiry { get; set; }
        public string? OtpIdentifier { get; set; }

        // Navigation Properties
        public Customer? Customer { get; set; }
        public Seller? Seller { get; set; }
    }
}