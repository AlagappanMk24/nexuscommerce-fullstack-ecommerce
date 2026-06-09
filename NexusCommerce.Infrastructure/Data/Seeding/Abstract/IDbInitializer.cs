namespace NexusCommerce.Infrastructure.Data.Seeding.Abstract
{
    public interface IDbInitializer
    {
        Task Initialize(CancellationToken cancellationToken);
    }
}