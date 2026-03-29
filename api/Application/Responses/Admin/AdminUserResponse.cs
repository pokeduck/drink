namespace Drink.Application.Responses.Admin;

public class AdminUserListResponse
{
  public int Id { get; set; }
  public string Username { get; set; } = null!;
  public int RoleId { get; set; }
  public string RoleName { get; set; } = null!;
  public bool IsActive { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class AdminUserDetailResponse
{
  public int Id { get; set; }
  public string Username { get; set; } = null!;
  public int RoleId { get; set; }
  public string RoleName { get; set; } = null!;
  public bool IsActive { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
