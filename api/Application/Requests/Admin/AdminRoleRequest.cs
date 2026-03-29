using System.ComponentModel.DataAnnotations;

namespace Drink.Application.Requests.Admin;

public class CreateAdminRoleRequest
{
  [Required]
  [StringLength(50)]
  public string Name { get; set; } = null!;

  public List<MenuCrudRequest> Menus { get; set; } = [];
}

public class UpdateAdminRoleRequest
{
  [Required]
  [StringLength(50)]
  public string Name { get; set; } = null!;

  public List<MenuCrudRequest> Menus { get; set; } = [];
}

public class MenuCrudRequest
{
  [Required]
  public int MenuId { get; set; }

  public bool CanRead { get; set; }
  public bool CanCreate { get; set; }
  public bool CanUpdate { get; set; }
  public bool CanDelete { get; set; }
}

public class DeleteAdminRoleRequest
{
  public int? ReassignRoleId { get; set; }
}
