using BankApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BankApi.Infrastructure.Persistence.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account> 
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(x=>x.Id);
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x=>x.Iban)
                .IsRequired()
                .HasMaxLength(34);

            builder.HasIndex(x => x.Iban)
                .IsUnique();

            builder.Property(x => x.Balance)
                .HasPrecision(18, 2);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.AccountType)
                .IsRequired();

            builder.HasOne(x=>x.Customer)
                .WithMany(c=>c.Accounts)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
