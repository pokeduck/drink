using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Drink.Application.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class MemberMapper
{
  public static partial MemberListResponse ToMemberListResponse(this User source);
  public static partial MemberDetailResponse ToMemberDetailResponse(this User source);
}
