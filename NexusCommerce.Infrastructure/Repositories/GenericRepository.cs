using Microsoft.EntityFrameworkCore;
using NexusCommerce.Application.Contracts.Persistence;
using NexusCommerce.Infrastructure.Data.Context;

namespace NexusCommerce.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository implementation for basic CRUD operations
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GenericRepository{T}"/> class
    /// </remarks>
    /// <param name="context">The database context</param>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    public class GenericRepository<T>(NexusCommerceContext context) : IGenericRepository<T> where T : class
    {
        protected readonly NexusCommerceContext _context = context ?? throw new ArgumentNullException(nameof(context));

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                return await _context.Set<T>().AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving all entities of type {typeof(T).Name}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<T>().FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving entity of type {typeof(T).Name} with ID: {id}", ex);
            }
        }

        /// <inheritdoc/>
        public void Add(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _context.Set<T>().Add(entity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error adding entity of type {typeof(T).Name}", ex);
            }
        }

        /// <inheritdoc/>
        public void Update(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _context.Set<T>().Update(entity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating entity of type {typeof(T).Name}", ex);
            }
        }

        /// <inheritdoc/>
        public void Delete(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _context.Set<T>().Remove(entity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting entity of type {typeof(T).Name}", ex);
            }
        }
    }
}