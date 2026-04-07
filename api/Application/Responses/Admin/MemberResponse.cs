using Drink.Domain.Enums;

namespace Drink.Application.Responses.Admin;

public class MemberListResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public string Email { get; set; } = null!;
  public string? Avatar { get; set; }
  public NotificationType NotificationType { get; set; }
  public UserStatus Status { get; set; }
  public bool EmailVerified { get; set; }
  public bool IsGoogleConnected { get; set; }
  public DateTime? LastLoginAt { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class MemberDetailResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public string Email { get; set; } = null!;
  public string? Avatar { get; set; }
  public NotificationType NotificationType { get; set; }
  public UserStatus Status { get; set; }
  public bool EmailVerified { get; set; }
  public bool IsGoogleConnected { get; set; }
  public DateTime? LastLoginAt { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
