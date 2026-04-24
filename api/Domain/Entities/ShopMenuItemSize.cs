using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class ShopMenuItemSize : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  public int MenuItemId { get; set; }
  public ShopMenuItem MenuItem { get; set; } = null!;

  public int SizeId { get; set; }
  public Size Size { get; set; } = null!;

  public decimal Price { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }
}
