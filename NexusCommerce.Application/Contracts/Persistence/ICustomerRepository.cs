using NexusCommerce.Domain.Entities;

namespace NexusCommerce.Application.Contracts.Persistence
{
    /// <summary>
    /// Repository interface for Customer-specific database operations
    /// </summary>
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        /// <summary>
        /// Retrieves all customers with their associated orders, order items, and products
        /// </summary>
        /// <returns>A collection of customers with their complete order history</returns>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<IEnumerable<Customer>> GetAllCustomersWithOrdersAsync();

        /// <summary>
        /// Retrieves all customers with their shopping carts
        /// </summary>
        /// <returns>A collection of customers with their cart information</returns>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<IEnumerable<Customer>> GetAllCustomersWithCartsAsync();

        /// <summary>
        /// Retrieves all customers with their wishlists
        /// </summary>
        /// <returns>A collection of customers with their wishlist information</returns>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<IEnumerable<Customer>> GetAllCustomersWithWishlistsAsync();

        /// <summary>
        /// Retrieves a specific customer by their unique identifier with user details
        /// </summary>
        /// <param name="id">The customer's unique identifier</param>
        /// <returns>The customer if found; null otherwise</returns>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<Customer?> GetCustomerByIdAsync(Guid id);

        /// <summary>
        /// Retrieves all customers with their associated user details
        /// </summary>
        /// <returns>A collection of customers with user information</returns>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        new Task<IEnumerable<Customer>> GetAllAsync();

        /// <summary>
        /// Retrieves a customer by their email address
        /// </summary>
        /// <param name="email">The customer's email address</param>
        /// <returns>The customer if found; null otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when email is null or empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<Customer?> GetCustomerByEmailAsync(string email);

        /// <summary>
        /// Retrieves a customer by their associated user ID
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <returns>The customer if found; null otherwise</returns>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<Customer?> GetCustomerByUserIdAsync(Guid userId);

        /// <summary>
        /// Soft deletes a customer by marking their user account as deleted and removing the customer record
        /// </summary>
        /// <param name="id">The customer's unique identifier</param>
        /// <returns>True if deletion was successful; false if customer or user not found</returns>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<bool> DeleteCustomerByID(Guid id);
    }
}