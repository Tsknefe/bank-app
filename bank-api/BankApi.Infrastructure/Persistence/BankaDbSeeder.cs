using Microsoft.EntityFrameworkCore;

namespace BankApi.Infrastructure.Persistence;

public static class BankaDbSeeder
{
    public static async Task SeedAsync(BankaDbContext db)
    {
       

        await db.Database.ExecuteSqlRawAsync("SELECT 1;");
    }
}
