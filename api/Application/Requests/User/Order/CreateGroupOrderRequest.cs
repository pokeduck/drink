using System.ComponentModel.DataAnnotations;

namespace Drink.Application.Requests.User.Order;

public class CreateGroupOrderRequest
{
  [Required]
  [StringLength(100)]
  public string Title { get; set; } = null!;

  [Required]
  public int ShopId { get; set; }

  [Required]
  public DateTime Deadline { get; set; }

  [StringLength(500)]
  public string? Note { get; set; }
}
