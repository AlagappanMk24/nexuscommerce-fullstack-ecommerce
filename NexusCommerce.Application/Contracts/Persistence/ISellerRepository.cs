using NexusCommerce.Domain.Entities;

namespace NexusCommerce.Application.Contracts.Persistence
{
    public interface ISellerRepository : IGenericRepository<Seller>
    {
        Task<IEnumerable<Seller>> GetAllSellersAsync();
        Task<IEnumerable<Seller>> GetAllSellersWithProductsAsync();
        Task<Seller?> GetSellerByIdAsync(Guid id);
        Task<Seller?> GetSellerByEmailAsync(string email);
        Task<Seller?> GetSellerByUserIdAsync(Guid userId);
        Task<Seller?> GetSellerWithProductsByIdAsync(Guid id);
        Task<Seller?> GetSellerWithProductsByEmailAsync(string email);
        Task<Seller?> GetSellerByStoreNameAsync(string storeName);
    }
}