using System.ComponentModel.DataAnnotations;
using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class ShopCategory : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  public int ShopId { get; set; }
  public Shop Shop { get; set; } = null!;

  [StringLength(100)]
  public string Name { get; set; } = null!;

  public int Sort { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }

  public ICollection<ShopMenuItem> MenuItems { get; set; } = [];
}
