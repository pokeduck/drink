using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Drink.Application.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class ShopMapper
{
  [MapperIgnoreTarget(nameof(ShopListResponse.CategoryCount))]
  [MapperIgnoreTarget(nameof(ShopListResponse.MenuItemCount))]
  public static partial ShopListResponse ToShopListResponse(this Shop source);
  public static partial ShopDetailResponse ToShopDetailResponse(this Shop source);
}
