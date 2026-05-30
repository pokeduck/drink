using Drink.Application.Models;
using Drink.Application.Requests.User.Order;
using Drink.Application.Responses;
using Drink.Application.Responses.User.Order;
using Drink.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drink.User.API.Controllers;

[Authorize]
public class OrdersController : BaseController
{
  private readonly UserOrderService _service;

  public OrdersController(UserOrderService service)
  {
    _service = service;
  }

  [HttpGet]
  [ProducesResponseType(typeof(ApiResponse<PaginationList<UserOrderListItemResponse>>), 200)]
  public async Task<IActionResult> GetList([FromQuery] UserOrderListQuery query)
  {
    var result = await _service.ListAsync(query);
    return ApiOk(result.Data);
  }

  [HttpGet("{id}")]
  [ProducesResponseType(typeof(ApiResponse<UserOrderDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> GetDetail(int id)
  {
    var result = await _service.GetDetailAsync(id);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, ResolveStatus(result.Error));

    return ApiOk(result.Data);
  }

  [HttpPost]
  [ProducesResponseType(typeof(ApiResponse<int>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> Create([FromBody] CreateGroupOrderRequest request)
  {
    var result = await _service.CreateGroupOrderAsync(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, ResolveStatus(result.Error));

    return ApiOk(result.Data);
  }

  [HttpPut("{id}")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 403)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> Update(int id, [FromBody] UpdateGroupOrderRequest request)
  {
    var result = await _service.UpdateGroupOrderAsync(id, request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, ResolveStatus(result.Error));

    return ApiOk();
  }

  [HttpPut("{id}/cancel")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 403)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> Cancel(int id)
  {
    var result = await _service.CancelGroupOrderAsync(id);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, ResolveStatus(result.Error));

    return ApiOk();
  }

  [HttpPost("{groupOrderId}/items")]
  [ProducesResponseType(typeof(ApiResponse<int>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> CreateItem(int groupOrderId, [FromBody] CreateOrderItemRequest request)
  {
    var result = await _service.CreateItemAsync(groupOrderId, request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, ResolveStatus(result.Error));

    return ApiOk(result.Data);
  }

  [HttpPut("{groupOrderId}/items/{itemId}")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 403)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> UpdateItem(int groupOrderId, int itemId, [FromBody] UpdateOrderItemRequest request)
  {
    var result = await _service.UpdateItemAsync(groupOrderId, itemId, request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, ResolveStatus(result.Error));

    return ApiOk();
  }

  [HttpDelete("{groupOrderId}/items/{itemId}")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 403)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> DeleteItem(int groupOrderId, int itemId)
  {
    var result = await _service.DeleteItemAsync(groupOrderId, itemId);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, ResolveStatus(result.Error));

    return ApiOk();
  }

  private static int ResolveStatus(string? error) => error switch
  {
    "ORDER_NOT_FOUND" => 404,
    "NOT_INITIATOR" => 403,
    _ => 400,
  };
}
