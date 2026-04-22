using Drink.Domain.Entities;
using Drink.Infrastructure.Data;
using Drink.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Drink.Migrator.Seeders;

public class AdminUserSeeder : ISeeder
{
  public int Order => 2;

  public async Task Seed(DrinkDbContext context, IConfiguration configuration)
  {
    if (await context.Set<AdminUser>().AnyAsync()) return;

    var pepper = configuration["Security:Pepper"]
                 ?? throw new InvalidOperationException("Security:Pepper is not configured.");
    var now = DateTime.UtcNow;

    context.Set<AdminUser>().Add(new AdminUser
    {
      Username = "admin",
      PasswordHash = new Drink.Infrastructure.Helpers.Argon2PasswordHasher().HashPassword("admin", pepper),
      RoleId = 1,
      IsActive = true,
      CreatedAt = now,
      Creator = 0,
      UpdatedAt = now,
      Updater = 0
    });

    await context.SaveChangesAsync();
    Console.WriteLine("  - AdminUser seeded");
  }
}
