using System.ComponentModel.DataAnnotations;

namespace Drink.Infrastructure.Data;

/// <summary>
/// 帳號
/// </summary>
public sealed class Account : BaseDataEntity, ICreatedEntity, IUpdatedEntity
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
    /// 2FA綁定
    /// </summary>
    public bool IsTwoFactorBound { get; set; }

    /// <summary>
    /// 狀態 (啟用/停用)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 是否鎖定
    /// </summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最後更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
