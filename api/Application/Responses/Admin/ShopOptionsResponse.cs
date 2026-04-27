namespace Drink.Application.Responses.Admin;

public class ShopOptionsResponse
{
  public List<ShopSugarOptionItem> Sugars { get; set; } = [];
  public List<ShopIceOptionItem> Ices { get; set; } = [];
  public List<ShopToppingOptionItem> Toppings { get; set; } = [];
  public List<ShopSizeOptionItem> Sizes { get; set; } = [];
}

public class ShopSugarOptionItem
{
  public int SugarId { get; set; }
  public string SugarName { get; set; } = null!;
  public decimal DefaultPrice { get; set; }
  public bool IsEnabled { get; set; }
  public int Sort { get; set; }
}

public class ShopIceOptionItem
{
  public int IceId { get; set; }
  public string IceName { get; set; } = null!;
  public bool IsEnabled { get; set; }
  public int Sort { get; set; }
}

public class ShopToppingOptionItem
{
  public int ToppingId { get; set; }
  public string ToppingName { get; set; } = null!;
  public decimal DefaultPrice { get; set; }
  public bool IsEnabled { get; set; }
  public int Sort { get; set; }
}

public class ShopSizeOptionItem
{
  public int SizeId { get; set; }
  public string SizeName { get; set; } = null!;
  public bool IsEnabled { get; set; }
  public int Sort { get; set; }
}

public class ShopOptionsPreviewResponse
{
  public ShopOptionsNewlyDisabledIds NewlyDisabled { get; set; } = new();
  public List<ShopOptionsAffectedMenuItem> AffectedMenuItems { get; set; } = [];
  public int AffectedMenuItemsCount { get; set; }
}

public class ShopOptionsNewlyDisabledIds
{
  public List<int> SugarIds { get; set; } = [];
  public List<int> IceIds { get; set; } = [];
  public List<int> ToppingIds { get; set; } = [];
  public List<int> SizeIds { get; set; } = [];
}

public class ShopOptionsAffectedMenuItem
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public ShopOptionsRemovedOptions RemovedOptions { get; set; } = new();
}

public class ShopOptionsRemovedOptions
{
  public List<int> Sugars { get; set; } = [];
  public List<int> Ices { get; set; } = [];
  public List<int> Toppings { get; set; } = [];
  public List<int> Sizes { get; set; } = [];
}

public class UpdateShopOptionsResponse
{
  public int AffectedMenuItemsCount { get; set; }
}
