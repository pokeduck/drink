namespace Drink.Application.Responses.Admin;

public class MenuTreeResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public string? Icon { get; set; }
  public string? Endpoint { get; set; }
  public int Sort { get; set; }
  public bool CanRead { get; set; }
  public bool CanCreate { get; set; }
  public bool CanUpdate { get; set; }
  public bool CanDelete { get; set; }
  public List<MenuTreeResponse> Children { get; set; } = [];
}
