using Drink.Domain.Enums;

namespace Drink.Application.Responses.User.Order;

public class UserOrderListItemResponse
{
  public int Id { get; set; }
  public string Title { get; set; } = null!;
  public int ShopId { get; set; }
  public string ShopName { get; set; } = null!;
  public string InitiatorName { get; set; } = null!;
  public GroupOrderStatus Status { get; set; }
  public DateTime Deadline { get; set; }
  public int OrderItemCount { get; set; }
  public decimal TotalAmount { get; set; }
  public bool IsMine { get; set; }
  public bool IsJoined { get; set; }
  public DateTime CreatedAt { get; set; }
}
