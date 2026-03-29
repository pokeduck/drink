namespace Drink.Application.Requests;

public class PaginationRequest
{
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 20;
  public string? SortBy { get; set; }
  public string SortOrder { get; set; } = "desc";
  public string? Keyword { get; set; }
}