namespace Drink.Application.Responses.Admin;

public class SugarListResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public decimal DefaultPrice { get; set; }
  public int Sort { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class SugarDetailResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public decimal DefaultPrice { get; set; }
  public int Sort { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
