using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class ShopToppingOverride : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  public int ShopId { get; set; }
  public Shop Shop { get; set; } = null!;

  public int ToppingId { get; set; }
  public Topping Topping { get; set; } = null!;

  public decimal? Price { get; set; }
  public int? Sort { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }
}
