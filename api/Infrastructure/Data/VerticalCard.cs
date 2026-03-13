using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drink.Infrastructure.Data;

/// <summary>
/// 直式卡匣管理
/// </summary>
public class VerticalCard : BaseDataEntity, ICreatedEntity, IUpdatedEntity
{
    /// <summary>
    /// 卡匣編號
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// 卡匣名稱
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 卡匣圖片Url
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// 卡匣背面圖Url
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string BackImageUrl { get; set; } = string.Empty;

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

    /// <summary>
    /// 彈別
    /// </summary>
    public ICollection<SeasonVerticalCard> SeasonVerticalCards { get; set; } = new List<SeasonVerticalCard>();
}
