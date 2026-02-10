using BankApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankApi.Infrastructure.Persistence.Configurations;

public class DebitCardConfiguration : IEntityTypeConfiguration<DebitCard>
{
    public void Configure(EntityTypeBuilder<DebitCard> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CardNo)
            .IsRequired()
            .HasMaxLength(16);

        builder.HasIndex(x => x.CardNo).IsUnique();

        builder.Property(x => x.CvvHash)
            .IsRequired();

        builder.Property(x => x.CvvSalt)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(x => x.Account)
            .WithMany() 
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
