using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Drink.Application.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class AdminRoleMapper
{
  [MapProperty(new[] { nameof(AdminRole.Users), nameof(List<AdminUser>.Count) }, new[] { nameof(AdminRoleListResponse.StaffCount) })]
  public static partial AdminRoleListResponse ToAdminRoleListResponse(this AdminRole source);

  public static List<AdminRoleListResponse> ToAdminRoleListResponseList(this List<AdminRole> source)
      => source.Select(x => x.ToAdminRoleListResponse()).ToList();
}
