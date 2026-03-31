namespace Drink.Application.Responses.Admin;

public class DrinkItemListResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public int Sort { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class DrinkItemDetailResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public int Sort { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
