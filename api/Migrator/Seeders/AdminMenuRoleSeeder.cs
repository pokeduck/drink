using Drink.Domain.Entities;
using Drink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Drink.Migrator.Seeders;

public class AdminMenuRoleSeeder : ISeeder
{
  public int Order => 4;

  public async Task Seed(DrinkDbContext context, IConfiguration configuration)
  {
    if (await context.Set<AdminMenuRole>().AnyAsync()) return;

    var now = DateTime.UtcNow;

    // Admin Role 對所有葉節點 Menu 全開 CRUD
    var leafMenuIds = await context.Set<AdminMenu>()
        .Where(m => m.Endpoint != null || m.IsPermissionOnly)
        .Select(m => m.Id)
        .ToListAsync();

    var menuRoles = leafMenuIds.Select(menuId => new AdminMenuRole
    {
      RoleId = 1,
      MenuId = menuId,
      CanRead = true,
      CanCreate = true,
      CanUpdate = true,
      CanDelete = true,
      CreatedAt = now,
      Creator = 0,
      UpdatedAt = now,
      Updater = 0
    }).ToList();

    context.Set<AdminMenuRole>().AddRange(menuRoles);
    await context.SaveChangesAsync();
    Console.WriteLine("  - AdminMenuRole seeded");
  }
}
