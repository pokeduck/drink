using Drink.Domain.Enums;

namespace Drink.Application.Responses.User.Order;

public class UserOrderDetailResponse
{
  public int Id { get; set; }
  public string Title { get; set; } = null!;
  public int ShopId { get; set; }
  public string ShopName { get; set; } = null!;
  public string InitiatorName { get; set; } = null!;
  public bool IsMine { get; set; }
  public GroupOrderStatus Status { get; set; }
  public DateTime Deadline { get; set; }
  public string? Note { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public List<UserOrderItemResponse> OrderItems { get; set; } = [];
  public UserOrderSummary Summary { get; set; } = null!;
}

public class UserOrderSummary
{
  public int TotalItems { get; set; }
  public decimal TotalAmount { get; set; }
  public int RecipientCount { get; set; }
}
