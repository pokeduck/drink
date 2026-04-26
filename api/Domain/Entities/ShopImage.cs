using System.ComponentModel.DataAnnotations;
using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class ShopImage : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  public int ShopId { get; set; }
  public Shop Shop { get; set; } = null!;

  public int? DrinkItemId { get; set; }
  public DrinkItem? DrinkItem { get; set; }

  [Required]
  [StringLength(255)]
  public string Path { get; set; } = null!;

  [Required]
  [StringLength(64)]
  public string Hash { get; set; } = null!;

  public int Sort { get; set; }

  public bool IsCover { get; set; }

  [StringLength(255)]
  public string? OriginalFileName { get; set; }

  public long FileSize { get; set; }

  public int Width { get; set; }

  public int Height { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }
}
