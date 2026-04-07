using System.ComponentModel.DataAnnotations;
using Drink.Domain.Enums;

namespace Drink.Application.Requests.Admin;

public class CreateMemberRequest
{
  [Required]
  [StringLength(100)]
  public string Name { get; set; } = null!;

  [Required]
  [StringLength(200)]
  [EmailAddress]
  public string Email { get; set; } = null!;

  [Required]
  public string Password { get; set; } = null!;
}

public class UpdateMemberRequest
{
  [Required]
  [StringLength(100)]
  public string Name { get; set; } = null!;

  public string? Avatar { get; set; }

  [Required]
  public NotificationType NotificationType { get; set; }

  [Required]
  public UserStatus Status { get; set; }
}

public class ResetMemberPasswordRequest
{
  [Required]
  public string NewPassword { get; set; } = null!;
}
