using Drink.Domain.Enums;

namespace Drink.Application.Requests.Admin.Order;

public class AdminOrderListQuery
{
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 20;
  public string? SortBy { get; set; }
  public string SortOrder { get; set; } = "desc";
  public string? Keyword { get; set; }
  public GroupOrderStatus? Status { get; set; }
  public int? ShopId { get; set; }
  public DateTime? CreatedFrom { get; set; }
  public DateTime? CreatedTo { get; set; }
  public DateTime? DeadlineFrom { get; set; }
  public DateTime? DeadlineTo { get; set; }
}
