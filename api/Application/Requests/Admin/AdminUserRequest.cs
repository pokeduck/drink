using System.ComponentModel.DataAnnotations;

namespace Drink.Application.Requests.Admin;

public class CreateAdminUserRequest
{
  [Required]
  [StringLength(50)]
  public string Username { get; set; } = null!;

  [Required]
  public string Password { get; set; } = null!;

  [Required]
  public int RoleId { get; set; }

  public bool IsActive { get; set; } = true;
}

public class UpdateAdminUserRequest
{
  [Required]
  public int RoleId { get; set; }

  public bool IsActive { get; set; }
}

public class ResetAdminUserPasswordRequest
{
  [Required]
  public string NewPassword { get; set; } = null!;
}
