using BankApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankApi.Infrastructure.Persistence.Configurations;

public class CardTransactionConfiguration : IEntityTypeConfiguration<CardTransaction>
{
    public void Configure(EntityTypeBuilder<CardTransaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasOne(x => x.CreditCard)
            .WithMany()
            .HasForeignKey(x => x.CreditCardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DebitCard)
            .WithMany()
            .HasForeignKey(x => x.DebitCardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasCheckConstraint(
            "CK_card_tx_exactly_one_card",
            @"((""CreditCardId"" IS NOT NULL AND ""DebitCardId"" IS NULL)
            OR (""CreditCardId"" IS NULL AND ""DebitCardId"" IS NOT NULL))"
        );

        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => x.CreditCardId);
        builder.HasIndex(x => x.DebitCardId);
        builder.HasIndex(x => x.AccountId);
    }
}
