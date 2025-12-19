using System.ComponentModel.DataAnnotations.Schema;

namespace Drink.Infrastructure.Data;

/// <summary>
/// 管理員與角色的多對多關聯表
/// </summary>
public sealed class AdminUserRole : BaseDataEntity
{
  /// <summary>
  /// 管理員 ID
  /// </summary>
  public int AdminUserId { get; set; }

  [ForeignKey(nameof(AdminUserId))]
  public AdminUser AdminUser { get; set; } = null!;

  /// <summary>
  /// 角色 ID
  /// </summary>
  public int RoleId { get; set; }

  [ForeignKey(nameof(RoleId))]
  public Role Role { get; set; } = null!;
}