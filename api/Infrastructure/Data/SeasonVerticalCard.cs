using System.ComponentModel.DataAnnotations.Schema;

namespace Drink.Infrastructure.Data;

/// <summary>
/// Season and VerticalCard many-to-many join table
/// </summary>
public class SeasonVerticalCard
{
    /// <summary>
    /// 彈別Id
    /// </summary>
    public int SeasonId { get; set; }
    
    /// <summary>
    /// 彈別
    /// </summary>
    [ForeignKey(nameof(SeasonId))]
    public Season Season { get; set; } = null!;

    /// <summary>
    /// 直式卡匣Id
    /// </summary>
    public int VerticalCardId { get; set; }

    /// <summary>
    /// 直式卡匣
    /// </summary>
    [ForeignKey(nameof(VerticalCardId))]
    public VerticalCard VerticalCard { get; set; } = null!;
}
