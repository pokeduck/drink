using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Mappings;
using Drink.Application.Models;
using Drink.Application.Requests.User.Order;
using Drink.Application.Responses;
using Drink.Application.Responses.User.Order;
using Drink.Domain.Entities;
using Drink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Drink.Application.Services;

public class UserOrderService : BaseService
{
  private readonly IGenericRepository<GroupOrder> _orderRepo;
  private readonly IGenericRepository<OrderItem> _itemRepo;
  private readonly IGenericRepository<OrderItemTopping> _itemToppingRepo;
  private readonly IGenericRepository<ShopMenuItemSize> _menuSizeRepo;
  private readonly IGenericRepository<ShopSugarOverride> _sugarOverrideRepo;
  private readonly IGenericRepository<ShopToppingOverride> _toppingOverrideRepo;
  private readonly IGenericRepository<Sugar> _sugarRepo;
  private readonly IGenericRepository<Topping> _toppingRepo;
  private readonly IGenericRepository<Shop> _shopRepo;

  public UserOrderService(
    ICurrentUserContext currentUser,
    IGenericRepository<GroupOrder> orderRepo,
    IGenericRepository<OrderItem> itemRepo,
    IGenericRepository<OrderItemTopping> itemToppingRepo,
    IGenericRepository<ShopMenuItemSize> menuSizeRepo,
    IGenericRepository<ShopSugarOverride> sugarOverrideRepo,
    IGenericRepository<ShopToppingOverride> toppingOverrideRepo,
    IGenericRepository<Sugar> sugarRepo,
    IGenericRepository<Topping> toppingRepo,
    IGenericRepository<Shop> shopRepo) : base(currentUser)
  {
    _orderRepo = orderRepo;
    _itemRepo = itemRepo;
    _itemToppingRepo = itemToppingRepo;
    _menuSizeRepo = menuSizeRepo;
    _sugarOverrideRepo = sugarOverrideRepo;
    _toppingOverrideRepo = toppingOverrideRepo;
    _sugarRepo = sugarRepo;
    _toppingRepo = toppingRepo;
    _shopRepo = shopRepo;
  }

  public async Task<ApiResponse<PaginationList<UserOrderListItemResponse>>> ListAsync(UserOrderListQuery query)
  {
    var page = Math.Max(query.Page, 1);
    var pageSize = Math.Clamp(query.PageSize, 1, 100);
    var me = CurrentUserId;

    var q = _orderRepo.Query;

    var scope = (query.Scope ?? "public").ToLower();
    if (scope == "mine")
    {
      q = q.Where(g => g.InitiatorId == me || g.OrderItems.Any(i => i.UserId == me));
    }
    else
    {
      q = q.Where(g => g.Status == GroupOrderStatus.Active);
    }

    if (!string.IsNullOrWhiteSpace(query.Keyword))
    {
      var kw = query.Keyword;
      q = q.Where(g =>
        g.Title.Contains(kw) ||
        g.Initiator.Name.Contains(kw));
    }

    if (query.ShopId.HasValue)
      q = q.Where(g => g.ShopId == query.ShopId.Value);

    q = BuildOrder(q, query.SortBy, query.SortOrder);

    var total = await q.CountAsync();

    var items = await q
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .Select(g => new UserOrderListItemResponse
      {
        Id = g.Id,
        Title = g.Title,
        ShopId = g.ShopId,
        ShopName = g.Shop.Name,
        InitiatorName = g.Initiator.Name,
        Status = g.Status,
        Deadline = g.Deadline,
        OrderItemCount = g.OrderItems.Count(),
        TotalAmount = g.OrderItems.Sum(i => i.TotalPrice * i.Quantity),
        IsMine = g.InitiatorId == me,
        IsJoined = g.OrderItems.Any(i => i.UserId == me),
        CreatedAt = g.CreatedAt,
      })
      .ToListAsync();

    return Success(new PaginationList<UserOrderListItemResponse>
    {
      Items = items,
      Total = total,
      Page = page,
      PageSize = pageSize,
    });
  }

  public async Task<ApiResponse<UserOrderDetailResponse>> GetDetailAsync(int orderId)
  {
    var me = CurrentUserId;

    var order = await _orderRepo.Query
      .Include(g => g.Shop)
      .Include(g => g.Initiator)
      .Include(g => g.OrderItems)
        .ThenInclude(i => i.User)
      .Include(g => g.OrderItems)
        .ThenInclude(i => i.MenuItem)
          .ThenInclude(m => m.DrinkItem)
      .Include(g => g.OrderItems)
        .ThenInclude(i => i.Size)
      .Include(g => g.OrderItems)
        .ThenInclude(i => i.Sugar)
      .Include(g => g.OrderItems)
        .ThenInclude(i => i.Ice)
      .Include(g => g.OrderItems)
        .ThenInclude(i => i.Toppings)
          .ThenInclude(t => t.Topping)
      .FirstOrDefaultAsync(g => g.Id == orderId);

    if (order is null)
      return Fail<UserOrderDetailResponse>(ErrorCodes.OrderNotFound, "揪團不存在");

    var orderItems = order.OrderItems
      .OrderBy(i => i.CreatedAt)
      .Select(i =>
      {
        var dto = i.ToUserOrderItemResponse();
        dto.IsMine = i.UserId == me;
        return dto;
      })
      .ToList();

    var response = new UserOrderDetailResponse
    {
      Id = order.Id,
      Title = order.Title,
      ShopId = order.ShopId,
      ShopName = order.Shop.Name,
      InitiatorName = order.Initiator.Name,
      IsMine = order.InitiatorId == me,
      Status = order.Status,
      Deadline = order.Deadline,
      Note = order.Note,
      CreatedAt = order.CreatedAt,
      UpdatedAt = order.UpdatedAt,
      OrderItems = orderItems,
      Summary = new UserOrderSummary
      {
        TotalItems = orderItems.Count,
        TotalAmount = orderItems.Sum(i => i.TotalPrice * i.Quantity),
        RecipientCount = orderItems.Select(i => i.RecipientName).Distinct().Count(),
      },
    };

    return Success(response);
  }

  public async Task<ApiResponse<int>> CreateGroupOrderAsync(CreateGroupOrderRequest request)
  {
    if (request.Deadline <= DateTime.UtcNow.AddMinutes(5))
      return Fail<int>(ErrorCodes.InvalidDeadline, "截止時間必須在 5 分鐘之後");

    var shop = await _shopRepo.GetById(request.ShopId);
    if (shop is null || shop.Status != ShopStatus.Active)
      return Fail<int>(ErrorCodes.ShopNotAvailable, "店家未上架");

    var entity = new GroupOrder
    {
      Title = request.Title,
      ShopId = request.ShopId,
      InitiatorId = CurrentUserId,
      Status = GroupOrderStatus.Active,
      Deadline = ToUtc(request.Deadline),
      Note = request.Note,
    };

    await _orderRepo.Insert(entity);

    return Success(entity.Id);
  }

  public async Task<ApiResponse> UpdateGroupOrderAsync(int id, UpdateGroupOrderRequest request)
  {
    var order = await _orderRepo.GetById(id, tracking: true);
    if (order is null)
      return Fail(ErrorCodes.OrderNotFound, "揪團不存在");

    if (order.InitiatorId != CurrentUserId)
      return Fail(ErrorCodes.NotInitiator, "僅發起人可編輯");

    if (order.Status != GroupOrderStatus.Active)
      return Fail(ErrorCodes.OrderNotActive, "揪團非進行中狀態");

    if (request.Deadline <= DateTime.UtcNow.AddMinutes(5))
      return Fail(ErrorCodes.InvalidDeadline, "截止時間必須在 5 分鐘之後");

    order.Title = request.Title;
    order.Deadline = ToUtc(request.Deadline);
    order.Note = request.Note;

    await _orderRepo.Update(order);
    return Success();
  }

  public async Task<ApiResponse> CancelGroupOrderAsync(int id)
  {
    var order = await _orderRepo.GetById(id, tracking: true);
    if (order is null)
      return Fail(ErrorCodes.OrderNotFound, "揪團不存在");

    if (order.InitiatorId != CurrentUserId)
      return Fail(ErrorCodes.NotInitiator, "僅發起人可取消");

    if (order.Status is not GroupOrderStatus.Active and not GroupOrderStatus.Closed)
      return Fail(ErrorCodes.CannotCancelOrder, "只有 Active 或 Closed 狀態的揪團可以取消");

    order.Status = GroupOrderStatus.Cancelled;
    await _orderRepo.Update(order);
    return Success();
  }

  public async Task<ApiResponse<int>> CreateItemAsync(int groupOrderId, CreateOrderItemRequest request)
  {
    var order = await _orderRepo.GetById(groupOrderId);
    if (order is null)
      return Fail<int>(ErrorCodes.OrderNotFound, "揪團不存在");

    if (order.Status != GroupOrderStatus.Active)
      return Fail<int>(ErrorCodes.OrderNotActive, "揪團非進行中狀態");

    var prices = await SnapshotPricesAsync(request.MenuItemId, request.SizeId, request.SugarId, request.ToppingIds, order.ShopId);
    if (prices is null)
      return Fail<int>(ErrorCodes.ShopNotAvailable, "品項或選項不存在或未啟用");

    var (itemPrice, sugarPrice, toppings, toppingTotal) = prices.Value;

    var item = new OrderItem
    {
      GroupOrderId = groupOrderId,
      UserId = CurrentUserId,
      RecipientName = request.RecipientName,
      MenuItemId = request.MenuItemId,
      SizeId = request.SizeId,
      SugarId = request.SugarId,
      IceId = request.IceId,
      ItemPrice = itemPrice,
      SugarPrice = sugarPrice,
      ToppingPrice = toppingTotal,
      TotalPrice = itemPrice + sugarPrice + toppingTotal,
      Quantity = request.Quantity,
      Note = request.Note,
    };

    await _itemRepo.Insert(item);

    if (toppings.Count > 0)
    {
      var entities = toppings.Select(t => new OrderItemTopping
      {
        OrderItemId = item.Id,
        ToppingId = t.toppingId,
        Price = t.price,
      });
      await _itemToppingRepo.InsertRange(entities);
    }

    return Success(item.Id);
  }

  public async Task<ApiResponse> UpdateItemAsync(int groupOrderId, int itemId, UpdateOrderItemRequest request)
  {
    var order = await _orderRepo.GetById(groupOrderId);
    if (order is null)
      return Fail(ErrorCodes.OrderNotFound, "揪團不存在");

    var item = await _itemRepo.Query
      .Include(i => i.Toppings)
      .FirstOrDefaultAsync(i => i.Id == itemId && i.GroupOrderId == groupOrderId);
    if (item is null)
      return Fail(ErrorCodes.OrderNotFound, "飲料項目不存在");

    if (!CanModifyItem(item, order))
      return Fail(ErrorCodes.NotInitiator, "無權編輯此飲料");

    if (order.Status != GroupOrderStatus.Active)
      return Fail(ErrorCodes.OrderNotActive, "揪團非進行中狀態");

    var prices = await SnapshotPricesAsync(request.MenuItemId, request.SizeId, request.SugarId, request.ToppingIds, order.ShopId);
    if (prices is null)
      return Fail(ErrorCodes.ShopNotAvailable, "品項或選項不存在或未啟用");

    var (itemPrice, sugarPrice, toppings, toppingTotal) = prices.Value;

    if (item.Toppings.Count > 0)
      await _itemToppingRepo.DeleteRange(item.Toppings);

    item.RecipientName = request.RecipientName;
    item.MenuItemId = request.MenuItemId;
    item.SizeId = request.SizeId;
    item.SugarId = request.SugarId;
    item.IceId = request.IceId;
    item.ItemPrice = itemPrice;
    item.SugarPrice = sugarPrice;
    item.ToppingPrice = toppingTotal;
    item.TotalPrice = itemPrice + sugarPrice + toppingTotal;
    item.Quantity = request.Quantity;
    item.Note = request.Note;

    await _itemRepo.Update(item);

    if (toppings.Count > 0)
    {
      var entities = toppings.Select(t => new OrderItemTopping
      {
        OrderItemId = item.Id,
        ToppingId = t.toppingId,
        Price = t.price,
      });
      await _itemToppingRepo.InsertRange(entities);
    }

    return Success();
  }

  public async Task<ApiResponse> DeleteItemAsync(int groupOrderId, int itemId)
  {
    var order = await _orderRepo.GetById(groupOrderId);
    if (order is null)
      return Fail(ErrorCodes.OrderNotFound, "揪團不存在");

    var item = await _itemRepo.Query
      .FirstOrDefaultAsync(i => i.Id == itemId && i.GroupOrderId == groupOrderId);
    if (item is null)
      return Fail(ErrorCodes.OrderNotFound, "飲料項目不存在");

    if (!CanModifyItem(item, order))
      return Fail(ErrorCodes.NotInitiator, "無權刪除此飲料");

    if (order.Status != GroupOrderStatus.Active)
      return Fail(ErrorCodes.OrderNotActive, "揪團非進行中狀態");

    await _itemRepo.Delete(item);
    return Success();
  }

  private async Task<(decimal itemPrice, decimal sugarPrice, List<(int toppingId, decimal price)> toppings, decimal toppingTotal)?>
    SnapshotPricesAsync(int menuItemId, int sizeId, int sugarId, List<int> toppingIds, int shopId)
  {
    var size = await _menuSizeRepo.Query
      .FirstOrDefaultAsync(s => s.MenuItemId == menuItemId && s.SizeId == sizeId);
    if (size is null)
      return null;

    var sugar = await _sugarRepo.GetById(sugarId);
    if (sugar is null)
      return null;

    var sugarOverride = await _sugarOverrideRepo.Query
      .FirstOrDefaultAsync(o => o.ShopId == shopId && o.SugarId == sugarId);
    var sugarPrice = sugarOverride?.Price ?? sugar.DefaultPrice;

    var toppings = new List<(int toppingId, decimal price)>();
    if (toppingIds.Count > 0)
    {
      var distinctIds = toppingIds.Distinct().ToList();
      var toppingList = await _toppingRepo.Query
        .Where(t => distinctIds.Contains(t.Id))
        .ToListAsync();
      if (toppingList.Count != distinctIds.Count)
        return null;

      var overrides = await _toppingOverrideRepo.Query
        .Where(o => o.ShopId == shopId && distinctIds.Contains(o.ToppingId))
        .ToListAsync();

      foreach (var id in toppingIds)
      {
        var topping = toppingList.First(t => t.Id == id);
        var ov = overrides.FirstOrDefault(o => o.ToppingId == id);
        toppings.Add((id, ov?.Price ?? topping.DefaultPrice));
      }
    }

    var toppingTotal = toppings.Sum(t => t.price);
    return (size.Price, sugarPrice, toppings, toppingTotal);
  }

  private bool CanModifyItem(OrderItem item, GroupOrder group)
    => item.UserId == CurrentUserId || group.InitiatorId == CurrentUserId;

  private static IQueryable<GroupOrder> BuildOrder(IQueryable<GroupOrder> query, string? sortBy, string? sortOrder)
  {
    var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

    var ordered = sortBy?.ToLower() switch
    {
      "id"         => isDesc ? query.OrderByDescending(g => g.Id) : query.OrderBy(g => g.Id),
      "deadline"   => isDesc ? query.OrderByDescending(g => g.Deadline) : query.OrderBy(g => g.Deadline),
      "created_at" => isDesc ? query.OrderByDescending(g => g.CreatedAt) : query.OrderBy(g => g.CreatedAt),
      _            => query.OrderByDescending(g => g.CreatedAt),
    };

    return ordered.ThenBy(g => g.Id);
  }

  private static DateTime ToUtc(DateTime value) => value.Kind switch
  {
    DateTimeKind.Utc => value,
    DateTimeKind.Local => value.ToUniversalTime(),
    _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
  };
}
