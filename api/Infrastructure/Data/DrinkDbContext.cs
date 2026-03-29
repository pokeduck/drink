using Drink.Domain.Entities;
using Drink.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Drink.Infrastructure.Data;

public class DrinkDbContext : DbContext
{
  public DrinkDbContext(DbContextOptions<DrinkDbContext> options) : base(options)
  { }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    base.OnConfiguring(optionsBuilder);
    optionsBuilder.UseSnakeCaseNamingConvention();
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
  }
}