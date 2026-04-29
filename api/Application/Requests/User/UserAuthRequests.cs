using System.ComponentModel.DataAnnotations;

namespace Drink.Application.Requests.User;

public class UserRegisterRequest
{
  [Required]
  [StringLength(100)]
  public string Name { get; set; } = null!;

  [Required]
  [EmailAddress]
  [StringLength(200)]
  public string Email { get; set; } = null!;

  [Required]
  [MinLength(6)]
  [StringLength(200)]
  public string Password { get; set; } = null!;
}

public class UserVerifyEmailRequest
{
  [Required]
  public string Token { get; set; } = null!;
}

public class UserLoginRequest
{
  [Required]
  [EmailAddress]
  public string Email { get; set; } = null!;

  [Required]
  public string Password { get; set; } = null!;
}

public class UserRefreshTokenRequest
{
  [Required]
  public string RefreshToken { get; set; } = null!;
}

public class UserLogoutRequest
{
  [Required]
  public string RefreshToken { get; set; } = null!;
}

public class ChangeUserPasswordRequest
{
  [Required]
  public string OldPassword { get; set; } = null!;

  [Required]
  [MinLength(6)]
  [StringLength(200)]
  public string NewPassword { get; set; } = null!;
}
