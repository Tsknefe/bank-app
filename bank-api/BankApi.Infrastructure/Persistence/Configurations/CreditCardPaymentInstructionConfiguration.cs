using BankApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankApi.Infrastructure.Persistence.Configurations;

public class CreditCardPaymentInstructionConfiguration : IEntityTypeConfiguration<CreditCardPaymentInstruction>
{
    public void Configure(EntityTypeBuilder<CreditCardPaymentInstruction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.Property(x => x.ScheduledAtUtc).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasOne(x => x.CreditCard)
            .WithMany()
            .HasForeignKey(x => x.CreditCardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CreditCardId, x.ScheduledAtUtc })
            .IsUnique();
        builder.HasIndex(x => new { x.Status, x.ScheduledAtUtc });
        builder.HasIndex(x => x.CreditCardId);
        builder.HasIndex(x => x.AccountId);
    }
}
