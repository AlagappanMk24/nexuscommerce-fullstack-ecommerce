using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusCommerce.Domain.Entities;

namespace NexusCommerce.Infrastructure.Data.Configurations
{
    public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
    {
        public void Configure(EntityTypeBuilder<PaymentMethod> builder)
        {
            builder.ToTable("PaymentMethods");

            builder.Property(pm => pm.MethodName)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasMany(pm => pm.CustomerPayments)
                .WithOne(cp => cp.PaymentMethod)
                .HasForeignKey(cp => cp.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}