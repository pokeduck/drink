using System.ComponentModel.DataAnnotations;
using Drink.Domain.Enums;
using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class User : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  [Required]
  [StringLength(100)]
  public string Name { get; set; } = null!;

  [Required]
  [StringLength(200)]
  public string Email { get; set; } = null!;

  public string? PasswordHash { get; set; }

  public string? Avatar { get; set; }

  public NotificationType NotificationType { get; set; }

  public UserStatus Status { get; set; }

  public bool EmailVerified { get; set; }

  public bool IsGoogleConnected { get; set; }

  public DateTime? LastLoginAt { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }

  public ICollection<UserRefreshToken> RefreshTokens { get; set; } = [];
}
