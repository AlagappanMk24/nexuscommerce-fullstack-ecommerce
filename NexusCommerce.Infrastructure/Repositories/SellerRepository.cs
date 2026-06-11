using Microsoft.EntityFrameworkCore;
using NexusCommerce.Application.Contracts.Persistence;
using NexusCommerce.Domain.Entities;
using NexusCommerce.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexusCommerce.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Seller entity operations
    /// </summary>
    public class SellerRepository : GenericRepository<Seller>, ISellerRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SellerRepository"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
        public SellerRepository(NexusCommerceContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Seller>> GetAllSellersAsync()
        {
            try
            {
                return await _context.Sellers
                    .Include(s => s.User)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving all sellers.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Seller>> GetAllSellersWithProductsAsync()
        {
            try
            {
                return await _context.Sellers
                    .Include(s => s.User)
                    .Include(s => s.Products)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving sellers with products.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Seller?> GetSellerByIdAsync(Guid id)
        {
            try
            {
                return await _context.Sellers
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving seller with ID: {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Seller?> GetSellerByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email cannot be null or empty", nameof(email));

                return await _context.Sellers
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.User != null && s.User.Email == email.Trim().ToLowerInvariant());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving seller with email: {email}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Seller?> GetSellerByUserIdAsync(Guid userId)
        {
            try
            {
                return await _context.Sellers
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.UserId == userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving seller with User ID: {userId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Seller?> GetSellerWithProductsByIdAsync(Guid id)
        {
            try
            {
                return await _context.Sellers
                    .Include(s => s.User)
                    .Include(s => s.Products)
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving seller with products by ID: {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Seller?> GetSellerWithProductsByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email cannot be null or empty", nameof(email));

                return await _context.Sellers
                    .Include(s => s.User)
                    .Include(s => s.Products)
                    .FirstOrDefaultAsync(s => s.User != null && s.User.Email == email.Trim().ToLowerInvariant());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving seller with products by email: {email}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Seller?> GetSellerByStoreNameAsync(string storeName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(storeName))
                    throw new ArgumentException("Store name cannot be null or empty", nameof(storeName));

                return await _context.Sellers
                    .FirstOrDefaultAsync(s => s.StoreName == storeName.Trim());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving seller with store name: {storeName}", ex);
            }
        }
    }
}