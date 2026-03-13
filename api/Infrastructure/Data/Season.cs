using System.ComponentModel.DataAnnotations;
using Drink.Infrastructure.Data;

namespace Drink.Infrastructure.Data;

/// <summary>
/// 彈別內容管理
/// </summary>
public class Season : BaseDataEntity, ICreatedEntity, IUpdatedEntity
{
    /// <summary>
    /// 彈別標題
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 彈別內容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 上傳卡表Url
    /// </summary>
    [MaxLength(500)]
    public string? CardTableUrl { get; set; }

    /// <summary>
    /// 發布時間
    /// </summary>
    public DateTime PublishAt { get; set; }

    /// <summary>
    /// 狀態 (啟用/停用)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// Meta Title
    /// </summary>
    [MaxLength(200)]
    public string? MetaTitle { get; set; }

    /// <summary>
    /// Meta Description
    /// </summary>
    [MaxLength(500)]
    public string? MetaDescription { get; set; }

    /// <summary>
    /// Og Title
    /// </summary>
    [MaxLength(200)]
    public string? OgTitle { get; set; }

    /// <summary>
    /// Og Description
    /// </summary>
    [MaxLength(500)]
    public string? OgDescription { get; set; }

    /// <summary>
    /// Og image Url
    /// </summary>
    [MaxLength(500)]
    public string? OgImageUrl { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// 建立者
    /// </summary>
    [MaxLength(50)]
    public string CreatedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// 更新者
    /// </summary>
    [MaxLength(50)]
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation property
    /// <summary>
    /// 直式卡匣
    /// </summary>
    public ICollection<SeasonVerticalCard> SeasonVerticalCards { get; set; } = new List<SeasonVerticalCard>();
    
    /// <summary>
    /// 橫式卡匣
    /// </summary>
    public ICollection<SeasonHorizontalCard> SeasonHorizontalCards { get; set; } = new List<SeasonHorizontalCard>();
}
