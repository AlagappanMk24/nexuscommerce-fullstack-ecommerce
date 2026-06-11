namespace NexusCommerce.Application.Contracts.Persistence
{
    /// <summary>
    /// Unit of Work interface for managing database transactions and repository access
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Gets the customer repository for customer-related operations
        /// </summary>
        ICustomerRepository CustomerRepository { get; }

        /// <summary>
        /// Gets the seller repository for seller-related operations
        /// </summary>
        ISellerRepository SellerRepository { get; }

        /// <summary>
        /// Saves all changes made in the unit of work to the database
        /// </summary>
        /// <returns>A task representing the asynchronous save operation</returns>
        /// <exception cref="InvalidOperationException">Thrown when database save operation fails</exception>
        Task SaveAsync();

        /// <summary>
        /// Executes an action within a database transaction
        /// </summary>
        /// <param name="action">The action to execute within the transaction</param>
        /// <returns>A task representing the asynchronous transaction operation</returns>
        /// <exception cref="ArgumentNullException">Thrown when action is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when transaction fails and is rolled back</exception>
        Task ExecuteInTransactionAsync(Func<Task> action);
    }
}