namespace NexusCommerce.Application.Contracts.Persistence
{
    /// <summary>
    /// Generic repository interface for basic CRUD operations
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves all entities of type T
        /// </summary>
        /// <returns>A collection of all entities</returns>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Retrieves an entity by its integer identifier
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <returns>The entity if found; null otherwise</returns>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Adds a new entity to the repository
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        void Add(T entity);

        /// <summary>
        /// Updates an existing entity in the repository
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        void Update(T entity);

        /// <summary>
        /// Deletes an entity from the repository
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        void Delete(T entity);
    }
}