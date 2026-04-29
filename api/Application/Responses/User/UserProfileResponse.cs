using Drink.Domain.Enums;

namespace Drink.Application.Responses.User;

public class UserProfileResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public string Email { get; set; } = null!;
  public string? Avatar { get; set; }
  public NotificationType NotificationType { get; set; }
  public bool IsGoogleConnected { get; set; }
  public bool EmailVerified { get; set; }
  public DateTime CreatedAt { get; set; }
}
