namespace Drink.Application.Responses.Admin;

// --- Shop List/Detail ---

public class ShopListResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public string? Phone { get; set; }
  public string? Address { get; set; }
  public int Status { get; set; }
  public int Sort { get; set; }
  public int CategoryCount { get; set; }
  public int MenuItemCount { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class ShopDetailResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public string? Phone { get; set; }
  public string? Address { get; set; }
  public string? Note { get; set; }
  public string? CoverImagePath { get; set; }
  public string? ExternalUrl { get; set; }
  public int Status { get; set; }
  public int Sort { get; set; }
  public int MaxToppingPerItem { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}

public class ShopCoverImageUploadResponse
{
  public string CoverImagePath { get; set; } = null!;
}

// --- Admin Shop Menu ---

public class AdminShopMenuResponse
{
  public List<AdminShopMenuCategoryResponse> Categories { get; set; } = [];
  public List<AdminShopMenuSugarOverrideResponse> SugarOverrides { get; set; } = [];
  public List<AdminShopMenuToppingOverrideResponse> ToppingOverrides { get; set; } = [];

  // 店家啟用的選項池（取代原本的全域 pool）
  public List<AdminShopMenuOptionItem> EnabledSugars { get; set; } = [];
  public List<AdminShopMenuOptionItem> EnabledIces { get; set; } = [];
  public List<AdminShopMenuOptionItem> EnabledToppings { get; set; } = [];
  public List<AdminShopMenuOptionItem> EnabledSizes { get; set; } = [];
}

public class AdminShopMenuOptionItem
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public int Sort { get; set; }
}

public class AdminShopMenuCategoryResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public int Sort { get; set; }
  public List<AdminShopMenuItemResponse> Items { get; set; } = [];
}

public class AdminShopMenuItemResponse
{
  public int Id { get; set; }
  public int DrinkItemId { get; set; }
  public string DrinkItemName { get; set; } = null!;
  public string? Description { get; set; }
  public int Sort { get; set; }
  public int MaxToppingCount { get; set; }
  public List<AdminShopMenuItemSizeResponse> Sizes { get; set; } = [];
  public List<int> SugarIds { get; set; } = [];
  public List<int> IceIds { get; set; } = [];
  public List<int> ToppingIds { get; set; } = [];
}

public class AdminShopMenuItemSizeResponse
{
  public int SizeId { get; set; }
  public string SizeName { get; set; } = null!;
  public decimal Price { get; set; }
}

public class AdminShopMenuSugarOverrideResponse
{
  public int SugarId { get; set; }
  public string SugarName { get; set; } = null!;
  public decimal? Price { get; set; }
}

public class AdminShopMenuToppingOverrideResponse
{
  public int ToppingId { get; set; }
  public string ToppingName { get; set; } = null!;
  public decimal? Price { get; set; }
}

// --- Shop Override ---

public class ShopOverrideResponse
{
  public List<ShopSugarOverrideDetailResponse> SugarOverrides { get; set; } = [];
  public List<ShopToppingOverrideDetailResponse> ToppingOverrides { get; set; } = [];
}

public class ShopSugarOverrideDetailResponse
{
  public int SugarId { get; set; }
  public string SugarName { get; set; } = null!;
  public decimal DefaultPrice { get; set; }
  public decimal? OverridePrice { get; set; }
}

public class ShopToppingOverrideDetailResponse
{
  public int ToppingId { get; set; }
  public string ToppingName { get; set; } = null!;
  public decimal DefaultPrice { get; set; }
  public decimal? OverridePrice { get; set; }
}
