namespace Drink.Application.Responses.Admin;

public class MenuTreeResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public string? Icon { get; set; }
  public string? Endpoint { get; set; }
  public int Sort { get; set; }
  public List<MenuTreeResponse> Children { get; set; } = [];
}
