using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexusCommerce.Application.Contracts.Persistence;
using NexusCommerce.Domain.Entities;
using NexusCommerce.Domain.Entities.Identity;
using NexusCommerce.Infrastructure.Data.Context;

namespace NexusCommerce.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Customer entity operations
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CustomerRepository"/> class
    /// </remarks>
    /// <param name="context">The database context</param>
    /// <param name="userManager">The user manager for handling user operations</param>
    /// <exception cref="ArgumentNullException">Thrown when context or userManager is null</exception>
    public class CustomerRepository(NexusCommerceContext context, UserManager<ApplicationUser> userManager) : GenericRepository<Customer>(context), ICustomerRepository
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

        /// <inheritdoc/>
        public async Task<IEnumerable<Customer>> GetAllCustomersWithOrdersAsync()
        {
            try
            {
                return await _context.Customers
                    .Include(c => c.User)
                    .Include(c => c.Orders)
                        .ThenInclude(o => o.OrderItems)
                            .ThenInclude(i => i.Product)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving customers with orders.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Customer>> GetAllCustomersWithCartsAsync()
        {
            try
            {
                return await _context.Customers
                    .Include(c => c.Cart)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving customers with carts.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Customer>> GetAllCustomersWithWishlistsAsync()
        {
            try
            {
                return await _context.Customers
                    .Include(c => c.Wishlist)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving customers with wishlists.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Customer?> GetCustomerByIdAsync(Guid id)
        {
            try
            {
                return await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving customer with ID: {id}", ex);
            }
        }

        /// <inheritdoc/>
        public new async Task<IEnumerable<Customer>> GetAllAsync()
        {
            try
            {
                return await _context.Customers
                    .Include(c => c.User)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving all customers.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email cannot be null or empty", nameof(email));

                return await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.User != null && c.User.Email == email.Trim().ToLowerInvariant());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving customer with email: {email}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Customer?> GetCustomerByUserIdAsync(Guid userId)
        {
            try
            {
                return await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving customer with User ID: {userId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteCustomerByID(Guid id)
        {
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);

                if (customer is null)
                    return false;

                var user = await _userManager.FindByIdAsync(customer.UserId.ToString());

                if (user is null)
                    return false;

                user.IsDeleted = true;
                user.IsActive = false;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return false;

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting customer with ID: {id}", ex);
            }
        }
    }
}