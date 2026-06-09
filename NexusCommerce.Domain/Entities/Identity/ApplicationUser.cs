using NexusCommerce.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace NexusCommerce.Domain.Entities.Identity
{
    public class ApplicationUser : AppIdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public Customer? Customer { get; set; }
        public Seller? Seller { get; set; }
    }
}