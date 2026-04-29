using System.ComponentModel.DataAnnotations;
using Drink.Domain.Enums;

namespace Drink.Application.Requests.User;

public class UpdateUserProfileRequest
{
  [Required]
  [StringLength(100, MinimumLength = 1)]
  public string Name { get; set; } = null!;

  [StringLength(500)]
  public string? Avatar { get; set; }

  [Required]
  [EnumDataType(typeof(NotificationType))]
  public NotificationType NotificationType { get; set; }
}
