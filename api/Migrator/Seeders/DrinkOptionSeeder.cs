using Drink.Domain.Entities;
using Drink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Drink.Migrator.Seeders;

public class DrinkOptionSeeder : ISeeder
{
  public int Order => 10;

  public async Task Seed(DrinkDbContext context, IConfiguration configuration)
  {
    var now = DateTime.UtcNow;

    await SeedDrinkItems(context, now);
    await SeedSugars(context, now);
    await SeedIces(context, now);
    await SeedToppings(context, now);
    await SeedSizes(context, now);
  }

  private static async Task SeedDrinkItems(DrinkDbContext context, DateTime now)
  {
    if (await context.Set<DrinkItem>().AnyAsync()) return;

    context.Set<DrinkItem>().AddRange(
      new DrinkItem { Id = 1,  Name = "珍珠奶茶", Sort = 1,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new DrinkItem { Id = 2,  Name = "紅茶",     Sort = 2,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new DrinkItem { Id = 3,  Name = "綠茶",     Sort = 3,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new DrinkItem { Id = 4,  Name = "烏龍茶",   Sort = 4,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new DrinkItem { Id = 5,  Name = "青茶",     Sort = 5,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new DrinkItem { Id = 6,  Name = "鮮奶茶",   Sort = 6,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new DrinkItem { Id = 7,  Name = "冬瓜茶",   Sort = 7,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new DrinkItem { Id = 8,  Name = "檸檬茶",   Sort = 8,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new DrinkItem { Id = 9,  Name = "多多綠",   Sort = 9,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new DrinkItem { Id = 10, Name = "果汁",     Sort = 10, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 }
    );

    await context.SaveChangesAsync();
    await ResetSequence(context, "drink_item");
    Console.WriteLine("  - DrinkItem seeded");
  }

  private static async Task SeedSugars(DrinkDbContext context, DateTime now)
  {
    if (await context.Set<Sugar>().AnyAsync()) return;

    context.Set<Sugar>().AddRange(
      new Sugar { Id = 1, Name = "正常糖", DefaultPrice = 0, Sort = 1, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Sugar { Id = 2, Name = "少糖",   DefaultPrice = 0, Sort = 2, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Sugar { Id = 3, Name = "半糖",   DefaultPrice = 0, Sort = 3, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Sugar { Id = 4, Name = "微糖",   DefaultPrice = 0, Sort = 4, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Sugar { Id = 5, Name = "無糖",   DefaultPrice = 0, Sort = 5, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Sugar { Id = 6, Name = "加蜂蜜", DefaultPrice = 5, Sort = 6, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 }
    );

    await context.SaveChangesAsync();
    await ResetSequence(context, "sugar");
    Console.WriteLine("  - Sugar seeded");
  }

  private static async Task SeedIces(DrinkDbContext context, DateTime now)
  {
    if (await context.Set<Ice>().AnyAsync()) return;

    context.Set<Ice>().AddRange(
      new Ice { Id = 1, Name = "正常冰", Sort = 1, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Ice { Id = 2, Name = "少冰",   Sort = 2, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Ice { Id = 3, Name = "微冰",   Sort = 3, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Ice { Id = 4, Name = "去冰",   Sort = 4, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Ice { Id = 5, Name = "熱",     Sort = 5, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Ice { Id = 6, Name = "溫",     Sort = 6, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 }
    );

    await context.SaveChangesAsync();
    await ResetSequence(context, "ice");
    Console.WriteLine("  - Ice seeded");
  }

  private static async Task SeedToppings(DrinkDbContext context, DateTime now)
  {
    if (await context.Set<Topping>().AnyAsync()) return;

    context.Set<Topping>().AddRange(
      new Topping { Id = 1,  Name = "珍珠",   DefaultPrice = 10, Sort = 1,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Topping { Id = 2,  Name = "椰果",   DefaultPrice = 10, Sort = 2,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Topping { Id = 3,  Name = "仙草",   DefaultPrice = 10, Sort = 3,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Topping { Id = 4,  Name = "布丁",   DefaultPrice = 10, Sort = 4,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Topping { Id = 5,  Name = "蘆薈",   DefaultPrice = 10, Sort = 5,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Topping { Id = 6,  Name = "愛玉",   DefaultPrice = 10, Sort = 6,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Topping { Id = 7,  Name = "粉條",   DefaultPrice = 10, Sort = 7,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Topping { Id = 8,  Name = "芋圓",   DefaultPrice = 15, Sort = 8,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Topping { Id = 9,  Name = "白玉",   DefaultPrice = 10, Sort = 9,  CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Topping { Id = 10, Name = "奶蓋",   DefaultPrice = 15, Sort = 10, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 }
    );

    await context.SaveChangesAsync();
    await ResetSequence(context, "topping");
    Console.WriteLine("  - Topping seeded");
  }

  private static async Task SeedSizes(DrinkDbContext context, DateTime now)
  {
    if (await context.Set<Size>().AnyAsync()) return;

    context.Set<Size>().AddRange(
      new Size { Id = 1, Name = "小杯", Sort = 1, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Size { Id = 2, Name = "中杯", Sort = 2, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Size { Id = 3, Name = "大杯", Sort = 3, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Size { Id = 4, Name = "S",    Sort = 4, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Size { Id = 5, Name = "M",    Sort = 5, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Size { Id = 6, Name = "L",    Sort = 6, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new Size { Id = 7, Name = "XL",   Sort = 7, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 }
    );

    await context.SaveChangesAsync();
    await ResetSequence(context, "size");
    Console.WriteLine("  - Size seeded");
  }

  private static async Task ResetSequence(DrinkDbContext context, string tableName)
  {
    await context.Database.ExecuteSqlRawAsync(
      $"SELECT setval(pg_get_serial_sequence('{tableName}', 'id'), COALESCE((SELECT MAX(id) FROM {tableName}), 0) + 1, false)");
  }
}
