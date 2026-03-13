using System.ComponentModel.DataAnnotations.Schema;

namespace Drink.Infrastructure.Data;

/// <summary>
/// Season and HorizontalCard many-to-many join table
/// </summary>
public class SeasonHorizontalCard
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
    /// 橫式卡匣Id
    /// </summary>
    public int HorizontalCardId { get; set; }

    /// <summary>
    /// 橫式卡匣
    /// </summary>
    [ForeignKey(nameof(HorizontalCardId))]
    public HorizontalCard HorizontalCard { get; set; } = null!;
}
