using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexusCommerce.Application.Contracts.Persistence;
using NexusCommerce.Domain.Entities;
using NexusCommerce.Domain.Entities.Identity;
using NexusCommerce.Infrastructure.Data.Context;

namespace NexusCommerce.Infrastructure.Repositories
{
    public class CustomerRepository(NexusCommerceContext context,
                              UserManager<ApplicationUser> userManager) : GenericRepository<Customer>(context), ICustomerRepository
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        // returns customers and their orders
        public async Task<IEnumerable<Customer>> GetAllCustomersWithOrdersAsync()
        {
            return await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Orders)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(i => i.Product)
                .ToListAsync();
        }

        // returns customers and their carts
        public async Task<IEnumerable<Customer>> GetAllCustomersWithCartsAsync()
        {
            var customers = await _context.Customers.Include(c => c.Cart).ToListAsync();
            return customers;
        }

        // return customers along with thier wishlists
        public async Task<IEnumerable<Customer>> GetAllCustomersWithWishlistsAsync()
        {
            var customers = await _context.Customers.Include(c => c.Wishlist).ToListAsync();
            return customers;
        }

        public async Task<Customer> GetCustomerByIdAsync(Guid id)
        {
            return await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public new async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .Include(c => c.User)
                .ToListAsync();
        }

        public async Task<Customer> GetCustomerByEmailAsync(string email)
        {
            return await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.User.Email == email);
        }

        public async Task<Customer?> GetCustomerByUserIdAsync(Guid userId)
        {
            return await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<bool> DeleteCustomerByID(Guid id)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
            if (customer is null) return false;

            var user = await _userManager.FindByIdAsync(customer.UserId.ToString());
            if (user is null) return false;

            user.IsDeleted = true;
            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}