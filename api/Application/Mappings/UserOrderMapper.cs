using Drink.Application.Responses.User.Order;
using Drink.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Drink.Application.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class UserOrderMapper
{
  [MapProperty([nameof(OrderItemTopping.Topping), nameof(Topping.Name)], nameof(UserOrderItemToppingResponse.ToppingName))]
  public static partial UserOrderItemToppingResponse ToUserOrderItemToppingResponse(this OrderItemTopping source);

  [MapProperty([nameof(OrderItem.User), nameof(User.Name)], nameof(UserOrderItemResponse.UserName))]
  [MapProperty([nameof(OrderItem.MenuItem), nameof(ShopMenuItem.DrinkItem), nameof(DrinkItem.Name)], nameof(UserOrderItemResponse.MenuItemName))]
  [MapProperty([nameof(OrderItem.Size), nameof(Size.Name)], nameof(UserOrderItemResponse.SizeName))]
  [MapProperty([nameof(OrderItem.Sugar), nameof(Sugar.Name)], nameof(UserOrderItemResponse.SugarName))]
  [MapProperty([nameof(OrderItem.Ice), nameof(Ice.Name)], nameof(UserOrderItemResponse.IceName))]
  [MapProperty(nameof(OrderItem.Toppings), nameof(UserOrderItemResponse.Toppings))]
  [MapperIgnoreTarget(nameof(UserOrderItemResponse.IsMine))]
  public static partial UserOrderItemResponse ToUserOrderItemResponse(this OrderItem source);
}
