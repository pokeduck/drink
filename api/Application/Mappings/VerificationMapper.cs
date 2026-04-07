using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Drink.Application.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class VerificationMapper
{
  [MapProperty(nameof(VerificationEmail.User) + "." + nameof(User.Name), nameof(VerificationListResponse.UserName))]
  [MapProperty(nameof(VerificationEmail.User) + "." + nameof(User.Email), nameof(VerificationListResponse.UserEmail))]
  public static partial VerificationListResponse ToVerificationListResponse(this VerificationEmail source);
}
