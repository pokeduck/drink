using System.ComponentModel.DataAnnotations;

namespace Drink.Infrastructure.Data;

/// <summary>
/// 後台管理員
/// </summary>
public sealed class AdminUser : BaseDataEntity, ICreatedEntity, IUpdatedEntity
{
  /// <summary>
  /// 登入帳號
  /// </summary>
  [Required]
  [MaxLength(50)]
  public string Username { get; set; } = string.Empty;

  /// <summary>
  /// 密碼 Hash
  /// </summary>
  [Required]
  public string PasswordHash { get; set; } = string.Empty;

  /// <summary>
  /// 顯示名稱
  /// </summary>
  [Required]
  [MaxLength(50)]
  public string FullName { get; set; } = string.Empty;

  /// <summary>
  /// 電子郵件
  /// </summary>
  [Required]
  [MaxLength(100)]
  public string Email { get; set; } = string.Empty;

  /// <summary>
  /// 是否啟用
  /// </summary>
  public bool IsActive { get; set; } = true;

  /// <summary>
  /// 建立時間
  /// </summary>
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// 最後更新時間
  /// </summary>
  public DateTime UpdatedAt { get; set; }

  // ===== 導覽屬性 =====
  public ICollection<AdminUserRole> AdminUserRoles { get; set; } = new List<AdminUserRole>();
}