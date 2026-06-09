using NexusCommerce.Domain.Entities;

namespace NexusCommerce.Application.Contracts.Persistence
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<IEnumerable<Customer>> GetAllCustomersWithOrdersAsync();
        Task<IEnumerable<Customer>> GetAllCustomersWithCartsAsync();
        Task<IEnumerable<Customer>> GetAllCustomersWithWishlistsAsync();
        Task<Customer> GetCustomerByIdAsync(Guid id);
        Task<Customer> GetCustomerByEmailAsync(string email);
        Task<Customer?> GetCustomerByUserIdAsync(Guid userId);
        Task<bool> DeleteCustomerByID(Guid id);
        //Task<bool> UpdateCustomer(Customer customer);
    }
}
