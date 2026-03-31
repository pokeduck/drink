namespace Drink.Application.Responses.Admin;

public class IceListResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public int Sort { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class IceDetailResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public int Sort { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
