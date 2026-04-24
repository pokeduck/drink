using System.ComponentModel.DataAnnotations;
using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class ShopMenuItem : BaseDataEntity, ICreateEntity, IUpdateEntity, ISoftDeleteEntity
{
  public int CategoryId { get; set; }
  public ShopCategory Category { get; set; } = null!;

  public int DrinkItemId { get; set; }
  public DrinkItem DrinkItem { get; set; } = null!;

  [StringLength(200)]
  public string? Description { get; set; }

  public int Sort { get; set; }

  public int MaxToppingCount { get; set; } = 5;

  public bool IsDeleted { get; set; }
  public DateTime? DeletedAt { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }

  public ICollection<ShopMenuItemSize> Sizes { get; set; } = [];
  public ICollection<ShopMenuItemSugar> Sugars { get; set; } = [];
  public ICollection<ShopMenuItemIce> Ices { get; set; } = [];
  public ICollection<ShopMenuItemTopping> Toppings { get; set; } = [];
}
