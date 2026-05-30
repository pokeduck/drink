using System.ComponentModel.DataAnnotations;
using Drink.Domain.Enums;

namespace Drink.Application.Requests.Admin.Order;

public class UpdateGroupOrderStatusRequest
{
  [Required]
  public GroupOrderStatus Status { get; set; }
}
