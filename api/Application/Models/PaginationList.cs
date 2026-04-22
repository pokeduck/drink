namespace Drink.Application.Models;

public class PaginationList<T> where T : class
{
  public List<T> Items { get; set; } = [];
  public int Total { get; set; }
  public int Page { get; set; }
  public int PageSize { get; set; }
}
