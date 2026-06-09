using Microsoft.EntityFrameworkCore;
using NexusCommerce.Application.Contracts.Persistence;
using NexusCommerce.Infrastructure.Data.Context;

namespace NexusCommerce.Infrastructure.Repositories
{
    public class GenericRepository<T>(NexusCommerceContext context) : IGenericRepository<T>
        where T : class
    {
        protected readonly NexusCommerceContext _context = context;
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }
        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        public void Add(T entity)
        {
            _context.Set<T>().Add(entity);
        }
        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }
        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
    }
}