using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexusCommerce.Application.Contracts.Persistence;
using NexusCommerce.Domain.Entities;
using NexusCommerce.Domain.Entities.Identity;
using NexusCommerce.Infrastructure.Data.Context;

namespace NexusCommerce.Infrastructure.Repositories
{
    public class SellerRepository(
        NexusCommerceContext context
        ) : GenericRepository<Seller>(context), ISellerRepository
    {
        public async Task<IEnumerable<Seller>> GetAllSellersAsync()
        {
            return await _context.Sellers
                .Include(s => s.User)
                .ToListAsync();
        }
        public async Task<IEnumerable<Seller>> GetAllSellersWithProductsAsync()
        {
            return await _context.Sellers
                .Include(s => s.User)
                .Include(s => s.Products)
                .ToListAsync();
        }
        public async Task<Seller?> GetSellerByIdAsync(Guid id)
        {
            return await _context.Sellers
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<Seller?> GetSellerByEmailAsync(string email)
        {
            return await _context.Sellers
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == email);
        }
        public async Task<Seller?> GetSellerByUserIdAsync(Guid userId)
        {
            return await _context.Sellers
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
        public async Task<Seller?> GetSellerWithProductsByIdAsync(Guid id)
        {
            return await _context.Sellers
                .Include(s => s.User)
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<Seller?> GetSellerWithProductsByEmailAsync(string email)
        {
            return await _context.Sellers
                .Include(s => s.User)
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.User!.Email == email);
        }
        public async Task<Seller?> GetSellerByStoreNameAsync(string storeName)
        {
            return await _context.Sellers
                .FirstOrDefaultAsync(s => s.StoreName == storeName);
        }
    }
}