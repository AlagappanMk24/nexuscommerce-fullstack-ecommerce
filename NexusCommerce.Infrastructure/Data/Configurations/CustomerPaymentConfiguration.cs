using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusCommerce.Domain.Entities;

namespace NexusCommerce.Infrastructure.Data.Configurations
{
    public class CustomerPaymentConfiguration : IEntityTypeConfiguration<CustomerPayment>
    {
        public void Configure(EntityTypeBuilder<CustomerPayment> builder)
        {
            builder.ToTable("CustomerPayments");

            builder.HasOne(cp => cp.Customer)
                .WithMany()
                .HasForeignKey(cp => cp.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(cp => cp.PaymentMethod)
                .WithMany(pm => pm.CustomerPayments)
                .HasForeignKey(cp => cp.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}