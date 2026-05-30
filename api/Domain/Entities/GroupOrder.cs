using System.ComponentModel.DataAnnotations;
using Drink.Domain.Enums;
using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class GroupOrder : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  [Required]
  [StringLength(100)]
  public string Title { get; set; } = null!;

  public int ShopId { get; set; }
  public Shop Shop { get; set; } = null!;

  public int InitiatorId { get; set; }
  public User Initiator { get; set; } = null!;

  public GroupOrderStatus Status { get; set; }

  public DateTime Deadline { get; set; }

  [StringLength(500)]
  public string? Note { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }

  public ICollection<OrderItem> OrderItems { get; set; } = [];
}
