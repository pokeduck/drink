using System.ComponentModel.DataAnnotations;
using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class OrderItem : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  public int GroupOrderId { get; set; }
  public GroupOrder GroupOrder { get; set; } = null!;

  public int UserId { get; set; }
  public User User { get; set; } = null!;

  [Required]
  [StringLength(100)]
  public string RecipientName { get; set; } = null!;

  public int MenuItemId { get; set; }
  public ShopMenuItem MenuItem { get; set; } = null!;

  public int SizeId { get; set; }
  public Size Size { get; set; } = null!;

  public int SugarId { get; set; }
  public Sugar Sugar { get; set; } = null!;

  public int IceId { get; set; }
  public Ice Ice { get; set; } = null!;

  public decimal ItemPrice { get; set; }
  public decimal SugarPrice { get; set; }
  public decimal ToppingPrice { get; set; }
  public decimal TotalPrice { get; set; }

  public int Quantity { get; set; } = 1;

  [StringLength(200)]
  public string? Note { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }

  public ICollection<OrderItemTopping> Toppings { get; set; } = [];
}
