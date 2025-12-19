using System.ComponentModel.DataAnnotations.Schema;

namespace Drink.Infrastructure.Data;

public sealed class SideBar : BaseDataEntity, ICreatedEntity, IUpdatedEntity
{
  /// <summary>
  /// 顯示名稱
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Icon class，例如 "bi bi-speedometer"
  /// </summary>
  public string? Icon { get; set; }

  /// <summary>
  /// 對應頁面或路由
  /// 例如 "/admin/dashboard"
  /// </summary>
  public string? Page { get; set; }

  /// <summary>
  /// 父層 SideBar Id（Root 為 null）
  /// </summary>
  [ForeignKey(nameof(Parent))]
  public int? ParentSideBarId { get; set; }

  /// <summary>
  /// 排序用（數字越小越前面）
  /// </summary>
  public int Order { get; set; }

  /// <summary>
  /// 是否啟用（不啟用則不顯示）
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

  public SideBar? Parent { get; set; }

  public ICollection<SideBar> Children { get; set; } = new List<SideBar>();
}