using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Drink.Application.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class ShopImageMapper
{
  [MapProperty(nameof(ShopImage.DrinkItem), nameof(ShopImageResponse.DrinkItem))]
  public static partial ShopImageResponse ToShopImageResponse(this ShopImage source);

  public static List<ShopImageResponse> ToShopImageResponseList(this IEnumerable<ShopImage> source)
    => source.Select(x => x.ToShopImageResponse()).ToList();

  private static ShopImageDrinkItemSummary? MapDrinkItem(DrinkItem? item)
    => item is null ? null : new ShopImageDrinkItemSummary { Id = item.Id, Name = item.Name };
}
