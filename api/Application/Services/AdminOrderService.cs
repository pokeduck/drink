using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Mappings;
using Drink.Application.Models;
using Drink.Application.Requests.Admin.Order;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin.Order;
using Drink.Domain.Entities;
using Drink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Drink.Application.Services;

public class AdminOrderService : BaseService
{
  private readonly IGenericRepository<GroupOrder> _orderRepo;

  // allowed source → target transitions
  private static readonly Dictionary<GroupOrderStatus, HashSet<GroupOrderStatus>> ValidTransitions = new()
  {
    [GroupOrderStatus.Active]    = [GroupOrderStatus.Closed, GroupOrderStatus.Delivered, GroupOrderStatus.Cancelled],
    [GroupOrderStatus.Closed]    = [GroupOrderStatus.Active, GroupOrderStatus.Delivered, GroupOrderStatus.Cancelled],
    [GroupOrderStatus.Delivered] = [GroupOrderStatus.Active, GroupOrderStatus.Closed, GroupOrderStatus.Completed],
    [GroupOrderStatus.Completed] = [],
    [GroupOrderStatus.Cancelled] = [],
  };

  public AdminOrderService(
    ICurrentUserContext currentUser,
    IGenericRepository<GroupOrder> orderRepo) : base(currentUser)
  {
    _orderRepo = orderRepo;
  }

  public async Task<ApiResponse<PaginationList<AdminOrderListItemResponse>>> ListAsync(AdminOrderListQuery query)
  {
    var page = Math.Max(query.Page, 1);
    var pageSize = Math.Clamp(query.PageSize, 1, 100);

    var q = _orderRepo.Query;

    if (!string.IsNullOrWhiteSpace(query.Keyword))
    {
      var kw = query.Keyword;
      q = q.Where(g =>
        g.Title.Contains(kw) ||
        g.Initiator.Name.Contains(kw));
    }

    if (query.Status.HasValue)
      q = q.Where(g => g.Status == query.Status.Value);

    if (query.ShopId.HasValue)
      q = q.Where(g => g.ShopId == query.ShopId.Value);

    if (query.CreatedFrom.HasValue)
    {
      var createdFrom = ToUtc(query.CreatedFrom.Value);
      q = q.Where(g => g.CreatedAt >= createdFrom);
    }

    // half-open range: created_to=2026-05-30 means "anything < 2026-05-31",
    // so a single-day filter covers the whole day instead of only midnight
    if (query.CreatedTo.HasValue)
    {
      var createdToExclusive = ToUtc(query.CreatedTo.Value.Date.AddDays(1));
      q = q.Where(g => g.CreatedAt < createdToExclusive);
    }

    if (query.DeadlineFrom.HasValue)
    {
      var deadlineFrom = ToUtc(query.DeadlineFrom.Value);
      q = q.Where(g => g.Deadline >= deadlineFrom);
    }

    if (query.DeadlineTo.HasValue)
    {
      var deadlineToExclusive = ToUtc(query.DeadlineTo.Value.Date.AddDays(1));
      q = q.Where(g => g.Deadline < deadlineToExclusive);
    }

    q = BuildOrder(q, query.SortBy, query.SortOrder);

    var total = await q.CountAsync();

    var items = await q
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .Select(g => new AdminOrderListItemResponse
      {
        Id = g.Id,
        Title = g.Title,
        ShopId = g.ShopId,
        ShopName = g.Shop.Name,
        InitiatorId = g.InitiatorId,
        InitiatorName = g.Initiator.Name,
        Status = g.Status,
        Deadline = g.Deadline,
        OrderItemCount = g.OrderItems.Count(),
        TotalAmount = g.OrderItems.Sum(i => i.TotalPrice * i.Quantity),
        CreatedAt = g.CreatedAt,
      })
      .ToListAsync();

    return Success(new PaginationList<AdminOrderListItemResponse>
    {
      Items = items,
      Total = total,
      Page = page,
      PageSize = pageSize,
    });
  }

  public async Task<ApiResponse<AdminOrderDetailResponse>> GetDetailAsync(int orderId)
  {
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
      return Fail<AdminOrderDetailResponse>(ErrorCodes.OrderNotFound, "揪團不存在");

    var orderItems = order.OrderItems
      .OrderBy(i => i.CreatedAt)
      .Select(i => i.ToAdminOrderItemResponse())
      .ToList();

    var response = new AdminOrderDetailResponse
    {
      Id = order.Id,
      Title = order.Title,
      ShopId = order.ShopId,
      ShopName = order.Shop.Name,
      InitiatorId = order.InitiatorId,
      InitiatorName = order.Initiator.Name,
      Status = order.Status,
      Deadline = order.Deadline,
      Note = order.Note,
      CreatedAt = order.CreatedAt,
      UpdatedAt = order.UpdatedAt,
      OrderItems = orderItems,
      Summary = new AdminOrderSummary
      {
        TotalItems = orderItems.Count,
        TotalAmount = orderItems.Sum(i => i.TotalPrice * i.Quantity),
        RecipientCount = orderItems.Select(i => i.RecipientName).Distinct().Count(),
      },
    };

    return Success(response);
  }

  public async Task<ApiResponse> UpdateStatusAsync(int orderId, GroupOrderStatus target)
  {
    var order = await _orderRepo.GetById(orderId, tracking: true);
    if (order is null)
      return Fail(ErrorCodes.OrderNotFound, "揪團不存在");

    if (!ValidTransitions.TryGetValue(order.Status, out var allowed) || !allowed.Contains(target))
      return Fail(ErrorCodes.InvalidStatusTransition, $"無法從 {order.Status} 轉換至 {target}");

    order.Status = target;
    await _orderRepo.Update(order);

    return Success();
  }

  public async Task<ApiResponse> CancelAsync(int orderId)
  {
    var order = await _orderRepo.GetById(orderId, tracking: true);
    if (order is null)
      return Fail(ErrorCodes.OrderNotFound, "揪團不存在");

    if (order.Status is not GroupOrderStatus.Active and not GroupOrderStatus.Closed)
      return Fail(ErrorCodes.CannotCancelOrder, "只有 Active 或 Closed 狀態的揪團可以取消");

    order.Status = GroupOrderStatus.Cancelled;
    await _orderRepo.Update(order);

    return Success();
  }

  public async Task<GroupOrder?> GetOrderWithItemsAsync(int orderId)
  {
    return await _orderRepo.Query
      .Include(g => g.Shop)
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
  }

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

  // Postgres timestamptz requires UTC; query-string DateTime arrives as Unspecified.
  private static DateTime ToUtc(DateTime value) => value.Kind switch
  {
    DateTimeKind.Utc => value,
    DateTimeKind.Local => value.ToUniversalTime(),
    _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
  };
}
