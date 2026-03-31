using AutoMapper;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;

namespace Drink.Application.Mappings;

public class DrinkOptionProfile : Profile
{
  public DrinkOptionProfile()
  {
    // DrinkItem
    CreateMap<DrinkItem, DrinkItemListResponse>();
    CreateMap<DrinkItem, DrinkItemDetailResponse>();

    // Sugar
    CreateMap<Sugar, SugarListResponse>();
    CreateMap<Sugar, SugarDetailResponse>();

    // Ice
    CreateMap<Ice, IceListResponse>();
    CreateMap<Ice, IceDetailResponse>();

    // Topping
    CreateMap<Topping, ToppingListResponse>();
    CreateMap<Topping, ToppingDetailResponse>();

    // Size
    CreateMap<Size, SizeListResponse>();
    CreateMap<Size, SizeDetailResponse>();
  }
}
