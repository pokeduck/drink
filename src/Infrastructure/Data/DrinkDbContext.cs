using Microsoft.EntityFrameworkCore;

namespace Drink.Infrastructure.Data;

public class DrinkDbContext : DbContext
{
  public DrinkDbContext(DbContextOptions<DrinkDbContext> options) : base(options)
  { }

  public DbSet<AdminUser> AdminUsers { get; set; }
  public DbSet<Role> Roles { get; set; }
  public DbSet<SideBar> SideBars { get; set; }
  public DbSet<AdminUserRole> AdminUserRoles { get; set; }
  public DbSet<RoleSideBarPermission> RoleSideBarPermissions { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // 設定 AdminUserRole 的複合主鍵
    modelBuilder.Entity<AdminUserRole>()
      .HasKey(ur => new { ur.AdminUserId, ur.RoleId });

    // 設定 AdminUser 和 Role 的多對多關係
    modelBuilder.Entity<AdminUserRole>()
      .HasOne(ur => ur.AdminUser)
      .WithMany(u => u.AdminUserRoles)
      .HasForeignKey(ur => ur.AdminUserId);

    modelBuilder.Entity<AdminUserRole>()
      .HasOne(ur => ur.Role)
      .WithMany(r => r.AdminUserRoles)
      .HasForeignKey(ur => ur.RoleId);

    // 設定 RoleSideBarPermission 的複合主鍵
    modelBuilder.Entity<RoleSideBarPermission>()
      .HasKey(rp => new { rp.RoleId, rp.SideBarId });

    // 設定 Role 和 SideBar 的多對多關係
    modelBuilder.Entity<RoleSideBarPermission>()
      .HasOne(rp => rp.Role)
      .WithMany(r => r.RoleSideBarPermissions)
      .HasForeignKey(rp => rp.RoleId);

    modelBuilder.Entity<RoleSideBarPermission>()
      .HasOne(rp => rp.SideBar)
      .WithMany() // SideBar不需要反向導覽屬性到權限表
      .HasForeignKey(rp => rp.SideBarId);

    // 設定 SideBar 的自我參考關係
    modelBuilder.Entity<SideBar>()
      .HasOne(s => s.Parent)
      .WithMany(s => s.Children)
      .HasForeignKey(s => s.ParentSideBarId)
      .OnDelete(DeleteBehavior.Restrict); // 防止刪除父節點時級聯刪除子節點
  }
}