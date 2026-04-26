using System.ComponentModel.DataAnnotations;
using Drink.Application.Attributes;

namespace Drink.Application.Requests.Admin;

// --- Shop ---

public class CreateShopRequest
{
  [Required]
  [StringLength(100)]
  public string Name { get; set; } = null!;

  [StringLength(20)]
  public string? Phone { get; set; }

  [StringLength(200)]
  public string? Address { get; set; }

  [StringLength(500)]
  public string? Note { get; set; }

  [StringLength(500)]
  public string? CoverImagePath { get; set; }

  [StringLength(500)]
  [HttpUrl]
  public string? ExternalUrl { get; set; }

  [Required]
  public int Status { get; set; }

  [Required]
  public int Sort { get; set; }

  [Required]
  [Range(1, int.MaxValue)]
  public int MaxToppingPerItem { get; set; } = 1;
}

public class UpdateShopRequest
{
  [Required]
  [StringLength(100)]
  public string Name { get; set; } = null!;

  [StringLength(20)]
  public string? Phone { get; set; }

  [StringLength(200)]
  public string? Address { get; set; }

  [StringLength(500)]
  public string? Note { get; set; }

  [StringLength(500)]
  public string? CoverImagePath { get; set; }

  [StringLength(500)]
  [HttpUrl]
  public string? ExternalUrl { get; set; }

  [Required]
  public int Status { get; set; }

  [Required]
  public int Sort { get; set; }

  [Required]
  [Range(1, int.MaxValue)]
  public int MaxToppingPerItem { get; set; }
}

// --- Category ---

public class CreateShopCategoryRequest
{
  [Required]
  [StringLength(100)]
  public string Name { get; set; } = null!;

  [Required]
  public int Sort { get; set; }
}

public class UpdateShopCategoryRequest
{
  [Required]
  [StringLength(100)]
  public string Name { get; set; } = null!;

  [Required]
  public int Sort { get; set; }
}

// --- MenuItem ---

public class CreateShopMenuItemRequest
{
  public int? DrinkItemId { get; set; }

  [StringLength(100)]
  public string? DrinkItemName { get; set; }

  [StringLength(200)]
  public string? Description { get; set; }

  [Required]
  public int Sort { get; set; }

  [Required]
  [Range(1, int.MaxValue)]
  public int MaxToppingCount { get; set; } = 5;

  [Required]
  [MinLength(1)]
  public List<MenuItemSizeRequest> Sizes { get; set; } = [];

  [Required]
  public List<int> SugarIds { get; set; } = [];

  [Required]
  public List<int> IceIds { get; set; } = [];

  [Required]
  public List<int> ToppingIds { get; set; } = [];
}

public class UpdateShopMenuItemRequest
{
  public int? DrinkItemId { get; set; }

  [StringLength(100)]
  public string? DrinkItemName { get; set; }

  [StringLength(200)]
  public string? Description { get; set; }

  [Required]
  public int Sort { get; set; }

  [Required]
  [Range(1, int.MaxValue)]
  public int MaxToppingCount { get; set; }

  [Required]
  [MinLength(1)]
  public List<MenuItemSizeRequest> Sizes { get; set; } = [];

  [Required]
  public List<int> SugarIds { get; set; } = [];

  [Required]
  public List<int> IceIds { get; set; } = [];

  [Required]
  public List<int> ToppingIds { get; set; } = [];
}

public class MenuItemSizeRequest
{
  [Required]
  public int SizeId { get; set; }

  [Required]
  [Range(0.01, double.MaxValue, ErrorMessage = "價格必須大於 0")]
  public decimal Price { get; set; }
}

// --- Override ---

public class UpdateShopOverrideRequest
{
  [Required]
  public List<ShopSugarOverrideItem> SugarOverrides { get; set; } = [];

  [Required]
  public List<ShopToppingOverrideItem> ToppingOverrides { get; set; } = [];
}

public class ShopSugarOverrideItem
{
  [Required]
  public int SugarId { get; set; }

  [Range(0, double.MaxValue)]
  public decimal? Price { get; set; }

  public int? Sort { get; set; }
}

public class ShopToppingOverrideItem
{
  [Required]
  public int ToppingId { get; set; }

  [Range(0, double.MaxValue)]
  public decimal? Price { get; set; }

  public int? Sort { get; set; }
}
