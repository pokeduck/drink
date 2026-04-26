namespace Drink.Application.Responses.Admin;

public class ShopImageResponse
{
  public int Id { get; set; }
  public string Path { get; set; } = null!;
  public string Hash { get; set; } = null!;
  public int Width { get; set; }
  public int Height { get; set; }
  public long FileSize { get; set; }
  public string? OriginalFileName { get; set; }
  public bool IsCover { get; set; }
  public int Sort { get; set; }
  public ShopImageDrinkItemSummary? DrinkItem { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class ShopImageDrinkItemSummary
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
}

public class ShopImageListResponse
{
  public List<ShopImageResponse> Items { get; set; } = [];
  public int Total { get; set; }
  public int Page { get; set; }
  public int PageSize { get; set; }
}

public class ShopImageBatchDeleteResponse
{
  public int Deleted { get; set; }
}
