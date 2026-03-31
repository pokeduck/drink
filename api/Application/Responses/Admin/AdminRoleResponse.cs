namespace Drink.Application.Responses.Admin;

public class AdminRoleListResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public bool IsSystem { get; set; }
  public int StaffCount { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class AdminRoleDetailResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public bool IsSystem { get; set; }
  public List<AdminMenuRoleResponse> Menus { get; set; } = [];
}

public class AdminMenuRoleResponse
{
  public int MenuId { get; set; }
  public string MenuName { get; set; } = null!;
  public bool CanRead { get; set; }
  public bool CanCreate { get; set; }
  public bool CanUpdate { get; set; }
  public bool CanDelete { get; set; }
}
