using Drink.Domain.Enums;

namespace Drink.Application.Responses.Admin.Order;

public class AdminOrderDetailResponse
{
  public int Id { get; set; }
  public string Title { get; set; } = null!;
  public int ShopId { get; set; }
  public string ShopName { get; set; } = null!;
  public int InitiatorId { get; set; }
  public string InitiatorName { get; set; } = null!;
  public GroupOrderStatus Status { get; set; }
  public DateTime Deadline { get; set; }
  public string? Note { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public List<AdminOrderItemResponse> OrderItems { get; set; } = [];
  public AdminOrderSummary Summary { get; set; } = null!;
}

public class AdminOrderSummary
{
  public int TotalItems { get; set; }
  public decimal TotalAmount { get; set; }
  public int RecipientCount { get; set; }
}
