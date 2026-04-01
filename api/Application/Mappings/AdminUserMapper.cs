using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Drink.Application.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class AdminUserMapper
{
  [MapProperty(nameof(AdminUser.Role) + "." + nameof(AdminRole.Name), nameof(AdminUserListResponse.RoleName))]
  public static partial AdminUserListResponse ToAdminUserListResponse(this AdminUser source);

  public static List<AdminUserListResponse> ToAdminUserListResponseList(this List<AdminUser> source)
      => source.Select(x => x.ToAdminUserListResponse()).ToList();

  [MapProperty(nameof(AdminUser.Role) + "." + nameof(AdminRole.Name), nameof(AdminUserDetailResponse.RoleName))]
  public static partial AdminUserDetailResponse ToAdminUserDetailResponse(this AdminUser source);
}
