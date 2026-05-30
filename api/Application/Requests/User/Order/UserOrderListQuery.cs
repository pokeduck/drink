namespace Drink.Application.Requests.User.Order;

public class UserOrderListQuery
{
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 20;
  public string? SortBy { get; set; }
  public string SortOrder { get; set; } = "desc";
  public string? Keyword { get; set; }
  public int? ShopId { get; set; }
  public string Scope { get; set; } = "public";
}
