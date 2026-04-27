using System.ComponentModel.DataAnnotations;

namespace Drink.Application.Requests.Admin;

public class UpdateShopOptionsRequest
{
  [Required]
  public List<ShopEnabledSugarItem> Sugars { get; set; } = [];

  [Required]
  public List<ShopEnabledIceItem> Ices { get; set; } = [];

  [Required]
  public List<ShopEnabledToppingItem> Toppings { get; set; } = [];

  [Required]
  public List<ShopEnabledSizeItem> Sizes { get; set; } = [];
}

public class ShopEnabledSugarItem
{
  [Required]
  public int SugarId { get; set; }

  public int Sort { get; set; }
}

public class ShopEnabledIceItem
{
  [Required]
  public int IceId { get; set; }

  public int Sort { get; set; }
}

public class ShopEnabledToppingItem
{
  [Required]
  public int ToppingId { get; set; }

  public int Sort { get; set; }
}

public class ShopEnabledSizeItem
{
  [Required]
  public int SizeId { get; set; }

  public int Sort { get; set; }
}
