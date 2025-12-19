using System.ComponentModel.DataAnnotations;

namespace Drink.Infrastructure.Data;

/// <summary>
/// 角色
/// </summary>
public sealed class Role : BaseDataEntity, ICreatedEntity, IUpdatedEntity
{
  /// <summary>
  /// 角色名稱
  /// </summary>
  [Required]
  [MaxLength(50)]
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// 角色描述
  /// </summary>
  [MaxLength(200)]
  public string? Description { get; set; }

  /// <summary>
  /// 是否啟用
  /// </summary>
  public bool IsActive { get; set; } = true;

  /// <summary>
  /// 建立時間
  /// </summary>
  public DateTime CreatedAt { get; set; }

  /// <summary>
  /// 最後更新時間
  /// </summary>
  public DateTime UpdatedAt { get; set; }

  // ===== 導覽屬性 =====
  public ICollection<AdminUserRole> AdminUserRoles { get; set; } = new List<AdminUserRole>();
  public ICollection<RoleSideBarPermission> RoleSideBarPermissions { get; set; } = new List<RoleSideBarPermission>();
}