using System.ComponentModel.DataAnnotations;
using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class Shop : BaseDataEntity, ICreateEntity, IUpdateEntity, ISoftDeleteEntity
{
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
  public string? ExternalUrl { get; set; }

  public ShopStatus Status { get; set; }

  public int Sort { get; set; }

  public int MaxToppingPerItem { get; set; } = 1;

  public bool IsDeleted { get; set; }
  public DateTime? DeletedAt { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }

  public ICollection<ShopCategory> Categories { get; set; } = [];
  public ICollection<ShopSugarOverride> SugarOverrides { get; set; } = [];
  public ICollection<ShopToppingOverride> ToppingOverrides { get; set; } = [];
}
