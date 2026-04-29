using Drink.Application.Requests.User;
using Drink.Application.Responses.User;
using Drink.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Drink.Application.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class UserProfileMapper
{
  public static partial UserProfileResponse ToUserProfileResponse(this User source);

  public static void ApplyUpdate(this User target, UpdateUserProfileRequest source)
  {
    target.Name = source.Name;
    target.Avatar = source.Avatar;
    target.NotificationType = source.NotificationType;
  }
}
