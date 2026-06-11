using NexusCommerce.Application.Contracts.Persistence;
using NexusCommerce.Infrastructure.Data.Context;

namespace NexusCommerce.Infrastructure
{
    /// <summary>
    /// Unit of Work implementation for managing database transactions and repositories
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly NexusCommerceContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="customerRepository">The customer repository</param>
        /// <param name="sellerRepository">The seller repository</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
        public UnitOfWork(
            NexusCommerceContext context,
            ICustomerRepository customerRepository,
            ISellerRepository sellerRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            CustomerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            SellerRepository = sellerRepository ?? throw new ArgumentNullException(nameof(sellerRepository));
        }

        /// <inheritdoc/>
        public ICustomerRepository CustomerRepository { get; }

        /// <inheritdoc/>
        public ISellerRepository SellerRepository { get; }

        /// <inheritdoc/>
        public async Task SaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error saving changes to the database.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await action();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Transaction failed and was rolled back.", ex);
            }
        }
    }
}