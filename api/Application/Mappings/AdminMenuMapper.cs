using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Drink.Application.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class AdminMenuMapper
{
  [MapperIgnoreTarget(nameof(MenuTreeResponse.Children))]
  public static partial MenuTreeResponse ToMenuTreeResponse(this AdminMenu source);
}
