using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class ShopMenuItemIce : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  public int MenuItemId { get; set; }
  public ShopMenuItem MenuItem { get; set; } = null!;

  public int IceId { get; set; }
  public Ice Ice { get; set; } = null!;

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }
}
