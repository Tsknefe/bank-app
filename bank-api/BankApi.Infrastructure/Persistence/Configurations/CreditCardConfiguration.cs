using BankApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankApi.Infrastructure.Persistence.Configurations;

public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCard>
{
    public void Configure(EntityTypeBuilder<CreditCard> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CardNo)
            .IsRequired()
            .HasMaxLength(16);

        builder.HasIndex(x => x.CardNo).IsUnique();

        builder.Property(x => x.Cvv)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(x => x.ExpireAt)
            .IsRequired();

        builder.Property(x => x.Limit)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CurrentDebt)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasOne(x => x.Customer)
            .WithMany(c => c.CreditCards)   
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
