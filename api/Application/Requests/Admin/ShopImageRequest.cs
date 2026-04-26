using System.ComponentModel.DataAnnotations;

namespace Drink.Application.Requests.Admin;

public class ShopImageListQuery
{
  /// <summary>all / orphan / assigned</summary>
  public string? Filter { get; set; }
  public int? DrinkItemId { get; set; }
  public string? Keyword { get; set; }
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 20;
}

/// <summary>
/// PATCH 圖片屬性：所有欄位皆 optional，不傳代表不變。
/// 改綁品項時：若想改為孤兒，傳 ChangeDrinkItem=true 且 DrinkItemId=null。
/// 想綁到具體品項：ChangeDrinkItem=true 且 DrinkItemId=12。
/// 不傳 ChangeDrinkItem 或 false → 不改綁。
/// </summary>
public class ShopImageUpdateRequest
{
  public bool ChangeDrinkItem { get; set; }
  public int? DrinkItemId { get; set; }
  public bool? IsCover { get; set; }
  public int? Sort { get; set; }
}

public class ShopImageBatchDeleteRequest
{
  [Required]
  [MinLength(1)]
  public List<int> Ids { get; set; } = [];
}

public class ShopImageSortRequest
{
  [Required]
  public List<ShopImageSortItem> Items { get; set; } = [];
}

public class ShopImageSortItem
{
  [Required]
  public int Id { get; set; }

  [Required]
  public int Sort { get; set; }
}
