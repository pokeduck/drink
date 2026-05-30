using Drink.Application.Attributes;
using Drink.Application.Constants;
using Drink.Application.Models;
using Drink.Application.Requests.Admin.Order;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin.Order;
using Drink.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drink.Admin.API.Controllers;

[Authorize]
public class OrdersController : BaseController
{
  private readonly AdminOrderService _service;
  private readonly AdminOrderExportService _exportService;
  private readonly AdminOrderNotificationService _notifyService;

  public OrdersController(
    AdminOrderService service,
    AdminOrderExportService exportService,
    AdminOrderNotificationService notifyService)
  {
    _service = service;
    _exportService = exportService;
    _notifyService = notifyService;
  }

  [HttpGet]
  [RequireRole(MenuConstants.OrderList, CrudAction.Read)]
  [ProducesResponseType(typeof(ApiResponse<PaginationList<AdminOrderListItemResponse>>), 200)]
  public async Task<IActionResult> GetList([FromQuery] AdminOrderListQuery query)
  {
    var result = await _service.ListAsync(query);
    return ApiOk(result.Data);
  }

  [HttpGet("{orderId}")]
  [RequireRole(MenuConstants.OrderList, CrudAction.Read)]
  [ProducesResponseType(typeof(ApiResponse<AdminOrderDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> GetDetail(int orderId)
  {
    var result = await _service.GetDetailAsync(orderId);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);

    return ApiOk(result.Data);
  }

  [HttpPut("{orderId}/status")]
  [RequireRole(MenuConstants.OrderList, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] UpdateGroupOrderStatusRequest request)
  {
    var result = await _service.UpdateStatusAsync(orderId, request.Status);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "ORDER_NOT_FOUND" ? 404 : 400;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus);
    }

    return ApiOk();
  }

  [HttpPut("{orderId}/cancel")]
  [RequireRole(MenuConstants.OrderList, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> Cancel(int orderId)
  {
    var result = await _service.CancelAsync(orderId);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "ORDER_NOT_FOUND" ? 404 : 400;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus);
    }

    return ApiOk();
  }

  [HttpGet("{orderId}/export")]
  [RequireRole(MenuConstants.OrderList, CrudAction.Read)]
  public async Task<IActionResult> Export(int orderId)
  {
    var result = await _exportService.ExportAsync(orderId);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);

    var (bytes, fileName) = result.Data;
    return File(bytes,
      "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      fileName);
  }

  [HttpPost("{orderId}/notify")]
  [RequireRole(MenuConstants.OrderList, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse<AdminOrderNotifyResponse>), 200)]
  public async Task<IActionResult> Notify(int orderId)
  {
    var result = await _notifyService.NotifyAsync(orderId);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);

    return ApiOk(result.Data);
  }
}
