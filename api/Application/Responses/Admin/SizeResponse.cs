namespace Drink.Application.Responses.Admin;

public class SizeListResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public int Sort { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class SizeDetailResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public int Sort { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
