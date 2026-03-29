using System.ComponentModel.DataAnnotations;

namespace Drink.Application.Requests.Admin;

public class AdminLoginRequest
{
  [Required]
  public string Username { get; set; } = null!;

  [Required]
  public string Password { get; set; } = null!;
}

public class AdminRefreshTokenRequest
{
  [Required]
  public string RefreshToken { get; set; } = null!;
}

public class AdminLogoutRequest
{
  [Required]
  public string RefreshToken { get; set; } = null!;
}

public class AdminChangePasswordRequest
{
  [Required]
  public string OldPassword { get; set; } = null!;

  [Required]
  public string NewPassword { get; set; } = null!;
}
