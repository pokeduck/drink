using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Drink.Application.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class DrinkOptionMapper
{
  // DrinkItem
  public static partial DrinkItemListResponse ToDrinkItemListResponse(this DrinkItem source);
  public static partial DrinkItemDetailResponse ToDrinkItemDetailResponse(this DrinkItem source);

  // Sugar
  public static partial SugarListResponse ToSugarListResponse(this Sugar source);
  public static partial SugarDetailResponse ToSugarDetailResponse(this Sugar source);

  // Ice
  public static partial IceListResponse ToIceListResponse(this Ice source);
  public static partial IceDetailResponse ToIceDetailResponse(this Ice source);

  // Topping
  public static partial ToppingListResponse ToToppingListResponse(this Topping source);
  public static partial ToppingDetailResponse ToToppingDetailResponse(this Topping source);

  // Size
  public static partial SizeListResponse ToSizeListResponse(this Size source);
  public static partial SizeDetailResponse ToSizeDetailResponse(this Size source);
}
