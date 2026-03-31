using System.ComponentModel.DataAnnotations;
using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class Topping : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  [StringLength(100)]
  public string Name { get; set; } = null!;

  public decimal DefaultPrice { get; set; }

  public int Sort { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }
}
