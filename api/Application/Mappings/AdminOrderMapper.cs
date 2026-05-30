using Drink.Application.Responses.Admin.Order;
using Drink.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Drink.Application.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class AdminOrderMapper
{
  [MapProperty([nameof(OrderItemTopping.Topping), nameof(Topping.Name)], nameof(AdminOrderItemToppingResponse.ToppingName))]
  public static partial AdminOrderItemToppingResponse ToAdminOrderItemToppingResponse(this OrderItemTopping source);

  [MapProperty([nameof(OrderItem.User), nameof(User.Name)], nameof(AdminOrderItemResponse.UserName))]
  [MapProperty([nameof(OrderItem.MenuItem), nameof(ShopMenuItem.DrinkItem), nameof(DrinkItem.Name)], nameof(AdminOrderItemResponse.MenuItemName))]
  [MapProperty([nameof(OrderItem.Size), nameof(Size.Name)], nameof(AdminOrderItemResponse.SizeName))]
  [MapProperty([nameof(OrderItem.Sugar), nameof(Sugar.Name)], nameof(AdminOrderItemResponse.SugarName))]
  [MapProperty([nameof(OrderItem.Ice), nameof(Ice.Name)], nameof(AdminOrderItemResponse.IceName))]
  [MapProperty(nameof(OrderItem.Toppings), nameof(AdminOrderItemResponse.Toppings))]
  public static partial AdminOrderItemResponse ToAdminOrderItemResponse(this OrderItem source);
}
