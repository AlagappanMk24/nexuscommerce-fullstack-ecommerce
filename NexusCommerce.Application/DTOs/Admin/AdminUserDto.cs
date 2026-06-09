namespace NexusCommerce.Application.DTOs.Admin
{
    public class UserLookupResultDTO
    {
        public string AccountType { get; set; } = string.Empty;
        public int ApplicationUserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        //public CustomerDTO? Customer { get; set; }
        //public SellerDTO? Seller { get; set; }
    }

    public class SetUserActiveDTO
    {
        public bool IsActive { get; set; }
    }
}