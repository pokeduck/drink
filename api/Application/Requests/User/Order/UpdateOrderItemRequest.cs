using System.ComponentModel.DataAnnotations;

namespace Drink.Application.Requests.User.Order;

public class UpdateOrderItemRequest
{
  [Required]
  [StringLength(100)]
  public string RecipientName { get; set; } = null!;

  [Required]
  public int MenuItemId { get; set; }

  [Required]
  public int SizeId { get; set; }

  [Required]
  public int SugarId { get; set; }

  [Required]
  public int IceId { get; set; }

  public List<int> ToppingIds { get; set; } = [];

  [Range(1, 99)]
  public int Quantity { get; set; } = 1;

  [StringLength(200)]
  public string? Note { get; set; }
}
