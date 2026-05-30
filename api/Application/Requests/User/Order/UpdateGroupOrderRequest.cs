using System.ComponentModel.DataAnnotations;

namespace Drink.Application.Requests.User.Order;

public class UpdateGroupOrderRequest
{
  [Required]
  [StringLength(100)]
  public string Title { get; set; } = null!;

  [Required]
  public DateTime Deadline { get; set; }

  [StringLength(500)]
  public string? Note { get; set; }
}
