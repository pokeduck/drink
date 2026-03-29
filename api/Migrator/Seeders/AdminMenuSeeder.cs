using Drink.Domain.Entities;
using Drink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Drink.Migrator.Seeders;

public class AdminMenuSeeder : ISeeder
{
  public int Order => 3;

  public async Task Seed(DrinkDbContext context, IConfiguration configuration)
  {
    if (await context.Set<AdminMenu>().AnyAsync()) return;

    var now = DateTime.UtcNow;

    var menus = new List<AdminMenu>
    {
      // 後台帳號管理
      new() { Id = 1,  ParentId = null, Name = "後台帳號管理", Icon = "UserFilled",   Endpoint = null,                                   Sort = 1, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new() { Id = 2,  ParentId = 1,    Name = "帳號列表",     Icon = "Avatar",       Endpoint = "/admin-account/list",                  Sort = 1, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new() { Id = 3,  ParentId = 1,    Name = "角色管理",     Icon = "Lock",         Endpoint = "/admin-account/role",                  Sort = 2, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },

      // 會員管理
      new() { Id = 4,  ParentId = null, Name = "會員管理",       Icon = "User",    Endpoint = null,                                     Sort = 2, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new() { Id = 5,  ParentId = 4,    Name = "會員列表",       Icon = "List",    Endpoint = "/member/list",                           Sort = 1, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new() { Id = 6,  ParentId = 4,    Name = "註冊驗證信",     Icon = "Message", Endpoint = "/member/verification/register",           Sort = 2, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new() { Id = 22, ParentId = 4,    Name = "忘記密碼驗證信", Icon = "EditPen", Endpoint = "/member/verification/forgot-password",    Sort = 3, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },

      // 訂單管理
      new() { Id = 7,  ParentId = null, Name = "訂單管理", Icon = "Document",     Endpoint = null,          Sort = 3, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new() { Id = 8,  ParentId = 7,    Name = "訂單列表", Icon = "DocumentCopy", Endpoint = "/order/list", Sort = 1, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },

      // 店家管理
      new() { Id = 9,  ParentId = null, Name = "店家管理", Icon = "Shop",    Endpoint = null,             Sort = 4, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new() { Id = 10, ParentId = 9,    Name = "店家列表", Icon = "Store",   Endpoint = "/shop/list",     Sort = 1, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new() { Id = 17, ParentId = 9,    Name = "覆寫設定", Icon = "Setting", Endpoint = "/shop/override", Sort = 2, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },

      // 飲料選項
      new() { Id = 11, ParentId = null, Name = "飲料選項", Icon = "ColdDrink", Endpoint = null,                    Sort = 5, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new() { Id = 12, ParentId = 11,   Name = "通用品名", Icon = "Grape",     Endpoint = "/drink-option/item",    Sort = 1, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new() { Id = 13, ParentId = 11,   Name = "甜度定義", Icon = "Sugar",     Endpoint = "/drink-option/sugar",   Sort = 2, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new() { Id = 14, ParentId = 11,   Name = "冰塊定義", Icon = "IceCream",  Endpoint = "/drink-option/ice",     Sort = 3, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new() { Id = 15, ParentId = 11,   Name = "加料",     Icon = "Plus",      Endpoint = "/drink-option/topping", Sort = 4, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new() { Id = 16, ParentId = 11,   Name = "容量定義", Icon = "CoffeeCup", Endpoint = "/drink-option/size",    Sort = 5, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },

      // 通知管理
      new() { Id = 18, ParentId = null, Name = "通知管理", Icon = "Bell",          Endpoint = null,                     Sort = 6, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new() { Id = 19, ParentId = 18,   Name = "通知列表", Icon = "ChatDotRound",  Endpoint = "/notification/list",     Sort = 1, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
      new() { Id = 20, ParentId = 18,   Name = "揪團通知", Icon = "ChatLineRound", Endpoint = "/notification/by-group", Sort = 2, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },

      // 系統設定
      new() { Id = 21, ParentId = null, Name = "系統設定", Icon = "Setting", Endpoint = "/system/setting", Sort = 7, CreatedAt = now, Creator = 0, UpdatedAt = now, Updater = 0 },
    };

    context.Set<AdminMenu>().AddRange(menus);
    await context.SaveChangesAsync();
    Console.WriteLine("  - AdminMenu seeded");
  }
}
