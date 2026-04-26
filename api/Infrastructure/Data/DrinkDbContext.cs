using Drink.Domain.Entities;
using Drink.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Drink.Infrastructure.Data;

public class DrinkDbContext : DbContext
{
  public DrinkDbContext(DbContextOptions<DrinkDbContext> options) : base(options)
  { }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    base.OnConfiguring(optionsBuilder);
    optionsBuilder.UseSnakeCaseNamingConvention();
    optionsBuilder.ConfigureWarnings(w =>
      w.Ignore(RelationalEventId.PendingModelChangesWarning));
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.RegisterAllEntities();

    // AdminUser → AdminRole (多對一)
    modelBuilder.Entity<AdminUser>(entity =>
    {
      entity.HasIndex(e => e.Username).IsUnique();

      entity.HasOne(e => e.Role)
        .WithMany(r => r.Users)
        .HasForeignKey(e => e.RoleId)
        .OnDelete(DeleteBehavior.Restrict);
    });

    // AdminMenu 自關聯 (ParentId → Id)
    modelBuilder.Entity<AdminMenu>(entity =>
    {
      entity.HasOne(e => e.Parent)
        .WithMany(e => e.Children)
        .HasForeignKey(e => e.ParentId)
        .OnDelete(DeleteBehavior.Restrict);
    });

    // AdminMenuRole → AdminRole + AdminMenu
    modelBuilder.Entity<AdminMenuRole>(entity =>
    {
      entity.HasIndex(e => new { e.RoleId, e.MenuId }).IsUnique();

      entity.HasOne(e => e.Role)
        .WithMany(r => r.MenuRoles)
        .HasForeignKey(e => e.RoleId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(e => e.Menu)
        .WithMany(m => m.MenuRoles)
        .HasForeignKey(e => e.MenuId)
        .OnDelete(DeleteBehavior.Cascade);
    });

    // AdminRefreshToken → AdminUser
    modelBuilder.Entity<AdminRefreshToken>(entity =>
    {
      entity.HasIndex(e => e.Token).IsUnique();

      entity.HasOne(e => e.User)
        .WithMany(u => u.RefreshTokens)
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Cascade);
    });

    // User (Email 唯一，不分大小寫)
    modelBuilder.Entity<User>(entity =>
    {
      entity.HasIndex(e => e.Email).IsUnique();
    });

    // UserRefreshToken → User
    modelBuilder.Entity<UserRefreshToken>(entity =>
    {
      entity.HasIndex(e => e.Token).IsUnique();

      entity.HasOne(e => e.User)
        .WithMany(u => u.RefreshTokens)
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Cascade);
    });

    // VerificationEmail → User
    modelBuilder.Entity<VerificationEmail>(entity =>
    {
      entity.HasIndex(e => e.Token).IsUnique();

      entity.HasOne(e => e.User)
        .WithMany()
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Cascade);
    });

    // Shop（店家名稱唯一，僅未刪除資料）
    modelBuilder.Entity<Shop>(entity =>
    {
      entity.HasIndex(e => e.Name)
        .HasFilter("is_deleted = false")
        .IsUnique();
    });

    // ShopCategory → Shop
    modelBuilder.Entity<ShopCategory>(entity =>
    {
      entity.HasOne(e => e.Shop)
        .WithMany(s => s.Categories)
        .HasForeignKey(e => e.ShopId)
        .OnDelete(DeleteBehavior.Cascade);
    });

    // ShopMenuItem → ShopCategory + DrinkItem
    modelBuilder.Entity<ShopMenuItem>(entity =>
    {
      entity.HasOne(e => e.Category)
        .WithMany(c => c.MenuItems)
        .HasForeignKey(e => e.CategoryId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(e => e.DrinkItem)
        .WithMany()
        .HasForeignKey(e => e.DrinkItemId)
        .OnDelete(DeleteBehavior.Restrict);
    });

    // ShopMenuItemSize → ShopMenuItem + Size
    modelBuilder.Entity<ShopMenuItemSize>(entity =>
    {
      entity.HasOne(e => e.MenuItem)
        .WithMany(m => m.Sizes)
        .HasForeignKey(e => e.MenuItemId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(e => e.Size)
        .WithMany()
        .HasForeignKey(e => e.SizeId)
        .OnDelete(DeleteBehavior.Restrict);
    });

    // ShopMenuItemSugar → ShopMenuItem + Sugar
    modelBuilder.Entity<ShopMenuItemSugar>(entity =>
    {
      entity.HasOne(e => e.MenuItem)
        .WithMany(m => m.Sugars)
        .HasForeignKey(e => e.MenuItemId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(e => e.Sugar)
        .WithMany()
        .HasForeignKey(e => e.SugarId)
        .OnDelete(DeleteBehavior.Restrict);
    });

    // ShopMenuItemIce → ShopMenuItem + Ice
    modelBuilder.Entity<ShopMenuItemIce>(entity =>
    {
      entity.HasOne(e => e.MenuItem)
        .WithMany(m => m.Ices)
        .HasForeignKey(e => e.MenuItemId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(e => e.Ice)
        .WithMany()
        .HasForeignKey(e => e.IceId)
        .OnDelete(DeleteBehavior.Restrict);
    });

    // ShopMenuItemTopping → ShopMenuItem + Topping
    modelBuilder.Entity<ShopMenuItemTopping>(entity =>
    {
      entity.HasOne(e => e.MenuItem)
        .WithMany(m => m.Toppings)
        .HasForeignKey(e => e.MenuItemId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(e => e.Topping)
        .WithMany()
        .HasForeignKey(e => e.ToppingId)
        .OnDelete(DeleteBehavior.Restrict);
    });

    // ShopSugarOverride → Shop + Sugar
    modelBuilder.Entity<ShopSugarOverride>(entity =>
    {
      entity.HasIndex(e => new { e.ShopId, e.SugarId }).IsUnique();

      entity.HasOne(e => e.Shop)
        .WithMany(s => s.SugarOverrides)
        .HasForeignKey(e => e.ShopId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(e => e.Sugar)
        .WithMany()
        .HasForeignKey(e => e.SugarId)
        .OnDelete(DeleteBehavior.Restrict);
    });

    // ShopToppingOverride → Shop + Topping
    modelBuilder.Entity<ShopToppingOverride>(entity =>
    {
      entity.HasIndex(e => new { e.ShopId, e.ToppingId }).IsUnique();

      entity.HasOne(e => e.Shop)
        .WithMany(s => s.ToppingOverrides)
        .HasForeignKey(e => e.ShopId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(e => e.Topping)
        .WithMany()
        .HasForeignKey(e => e.ToppingId)
        .OnDelete(DeleteBehavior.Restrict);
    });

    // ShopImage → Shop + DrinkItem
    modelBuilder.Entity<ShopImage>(entity =>
    {
      entity.HasOne(e => e.Shop)
        .WithMany()
        .HasForeignKey(e => e.ShopId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(e => e.DrinkItem)
        .WithMany()
        .HasForeignKey(e => e.DrinkItemId)
        .OnDelete(DeleteBehavior.Restrict);

      // 每組 (ShopId, DrinkItemId) 至多一張 IsCover=true
      entity.HasIndex(e => new { e.ShopId, e.DrinkItemId })
        .HasFilter("is_cover = true")
        .IsUnique();

      // 列表排序用
      entity.HasIndex(e => new { e.ShopId, e.DrinkItemId, e.Sort });

      // 孤兒查詢用
      entity.HasIndex(e => e.ShopId)
        .HasFilter("drink_item_id IS NULL");

      // 跨表查相同 hash 用（非 unique）
      entity.HasIndex(e => e.Hash);
    });
  }
}