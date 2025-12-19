using System.ComponentModel.DataAnnotations.Schema;

namespace Drink.Infrastructure.Data;

/// <summary>
/// 角色與 SideBar 權限的多對多關聯表
/// </summary>
public sealed class RoleSideBarPermission : BaseDataEntity
{
  /// <summary>
  /// 角色 ID
  /// </summary>
  public int RoleId { get; set; }

  [ForeignKey(nameof(RoleId))]
  public Role Role { get; set; } = null!;

  /// <summary>
  /// SideBar ID
  /// </summary>
  public int SideBarId { get; set; }

  [ForeignKey(nameof(SideBarId))]
  public SideBar SideBar { get; set; } = null!;
}