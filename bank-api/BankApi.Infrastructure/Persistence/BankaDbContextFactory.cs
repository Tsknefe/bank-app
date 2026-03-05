using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankApi.Infrastructure.Persistence;

public class BankaDbContextFactory : IDesignTimeDbContextFactory<BankaDbContext>
{
    public BankaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BankaDbContext>();

        //! port 5433 var conn = "Host=localhost;Port=5432;Database=bankdb;Username=bankuser;Password=bankpass";
        var cs =
            Environment.GetEnvironmentVariable("BANKAPP_CONNECTION")
            ?? "Host=localhost;Port=5433;Database=bankdb;Username=bankuser;Password=bankpass";

        optionsBuilder.UseNpgsql(cs);

        return new BankaDbContext(optionsBuilder.Options);
    }
}
