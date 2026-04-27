using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class ShopEnabledSize : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  public int ShopId { get; set; }
  public Shop Shop { get; set; } = null!;

  public int SizeId { get; set; }
  public Size Size { get; set; } = null!;

  public int Sort { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }
}
