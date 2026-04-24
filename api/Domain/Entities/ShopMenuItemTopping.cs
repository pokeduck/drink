using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class ShopMenuItemTopping : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  public int MenuItemId { get; set; }
  public ShopMenuItem MenuItem { get; set; } = null!;

  public int ToppingId { get; set; }
  public Topping Topping { get; set; } = null!;

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }
}
