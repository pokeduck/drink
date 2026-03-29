using Drink.Domain.Entities;
using Drink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Drink.Migrator.Seeders;

public class AdminRoleSeeder : ISeeder
{
  public int Order => 1;

  public async Task Seed(DrinkDbContext context, IConfiguration configuration)
  {
    if (await context.Set<AdminRole>().AnyAsync()) return;

    var now = DateTime.UtcNow;

    context.Set<AdminRole>().Add(new AdminRole
    {
      Id = 1,
      Name = "Admin",
      IsSystem = true,
      CreatedAt = now,
      Creator = 0,
      UpdatedAt = now,
      Updater = 0
    });

    await context.SaveChangesAsync();
    Console.WriteLine("  - AdminRole seeded");
  }
}
