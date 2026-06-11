using NexusCommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexusCommerce.Application.Contracts.Persistence
{
    /// <summary>
    /// Repository interface for Seller-specific database operations
    /// </summary>
    public interface ISellerRepository : IGenericRepository<Seller>
    {
        /// <summary>
        /// Retrieves all sellers with their associated user details
        /// </summary>
        /// <returns>A collection of all sellers with user information</returns>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<IEnumerable<Seller>> GetAllSellersAsync();

        /// <summary>
        /// Retrieves all sellers with their associated products
        /// </summary>
        /// <returns>A collection of sellers with their product listings</returns>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<IEnumerable<Seller>> GetAllSellersWithProductsAsync();

        /// <summary>
        /// Retrieves a specific seller by their unique identifier
        /// </summary>
        /// <param name="id">The seller's unique identifier</param>
        /// <returns>The seller if found; null otherwise</returns>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<Seller?> GetSellerByIdAsync(Guid id);

        /// <summary>
        /// Retrieves a seller by their email address
        /// </summary>
        /// <param name="email">The seller's email address</param>
        /// <returns>The seller if found; null otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when email is null or empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<Seller?> GetSellerByEmailAsync(string email);

        /// <summary>
        /// Retrieves a seller by their associated user ID
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <returns>The seller if found; null otherwise</returns>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<Seller?> GetSellerByUserIdAsync(Guid userId);

        /// <summary>
        /// Retrieves a seller with their complete product catalog
        /// </summary>
        /// <param name="id">The seller's unique identifier</param>
        /// <returns>The seller with their products if found; null otherwise</returns>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<Seller?> GetSellerWithProductsByIdAsync(Guid id);

        /// <summary>
        /// Retrieves a seller by email with their complete product catalog
        /// </summary>
        /// <param name="email">The seller's email address</param>
        /// <returns>The seller with their products if found; null otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when email is null or empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<Seller?> GetSellerWithProductsByEmailAsync(string email);

        /// <summary>
        /// Retrieves a seller by their store name
        /// </summary>
        /// <param name="storeName">The store name to search for</param>
        /// <returns>The seller if found; null otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when store name is null or empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<Seller?> GetSellerByStoreNameAsync(string storeName);
    }
}