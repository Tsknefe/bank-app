using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Infrastructure.Persistence
{
    public class BankaDbContext : DbContext
    {
        public BankaDbContext(DbContextOptions<BankaDbContext> options) : base(options) { }
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<DebitCard> DebitCards => Set<DebitCard>();
        public DbSet<CreditCard> CreditCards => Set<CreditCard>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BankaDbContext).Assembly);


            modelBuilder.Entity<Customer>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.FirstName).IsRequired().HasMaxLength(200);

            });
            modelBuilder.Entity<Account>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Iban).IsRequired().HasMaxLength(34);
            });
            modelBuilder.Entity<Customer>().HasQueryFilter(x => x.IsActive);

        }

    }
}
