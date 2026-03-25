using System.ComponentModel.DataAnnotations;
using Drink.Domain.Enums;
using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class Account : BaseDataEntity, ICreatedEntity
{
  public ActivationStatus ActivationStatus { get; set; }
  public DateTime CreatedAt { get; set; }
  public long UpdatedAt { get; set; }
  public bool IsDeleted { get; set; }

  [StringLength(100)]
  public string Name { get; set; }

  [StringLength(100)]
  public string? Nickname { get; set; }
}