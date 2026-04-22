using Drink.Application.Attributes;
using Drink.Application.Constants;
using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Application.Services;
using Drink.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drink.Admin.API.Controllers;

[Authorize]
public class ToppingsController : BaseController
{
  private readonly DrinkOptionService _service;

  public ToppingsController(DrinkOptionService service)
  {
    _service = service;
  }

  [HttpGet]
  [RequireRole(MenuConstants.Topping, CrudAction.Read)]
  [ProducesResponseType(typeof(ApiResponse<PaginationList<ToppingListResponse>>), 200)]
  public async Task<IActionResult> GetList(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? sortBy = null,
    [FromQuery] string? sortOrder = null,
    [FromQuery] string? keyword = null)
  {
    var result = await _service.GetToppingList(page, pageSize, sortBy, sortOrder, keyword);
    return ApiOk(result.Data);
  }

  [HttpGet("{id}")]
  [RequireRole(MenuConstants.Topping, CrudAction.Read)]
  [ProducesResponseType(typeof(ApiResponse<ToppingDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> GetById(int id)
  {
    var result = await _service.GetToppingById(id);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk(result.Data);
  }

  [HttpPost]
  [RequireRole(MenuConstants.Topping, CrudAction.Create)]
  [ProducesResponseType(typeof(ApiResponse<ToppingDetailResponse>), 201)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> Create([FromBody] CreateToppingRequest request)
  {
    var result = await _service.CreateTopping(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 409, result.Errors);
    return StatusCode(201, ApiResponse<ToppingDetailResponse>.Success(result.Data));
  }

  [HttpPut("{id}")]
  [RequireRole(MenuConstants.Topping, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse<ToppingDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> Update(int id, [FromBody] UpdateToppingRequest request)
  {
    var result = await _service.UpdateTopping(id, request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "TOPPING_ALREADY_EXISTS" ? 409 : 404;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return ApiOk(result.Data);
  }

  [HttpDelete("{id}")]
  [RequireRole(MenuConstants.Topping, CrudAction.Delete)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> Delete(int id)
  {
    var result = await _service.DeleteTopping(id);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "TOPPING_IN_USE" ? 400 : 404;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return ApiOk();
  }

  [HttpPut("sort")]
  [RequireRole(MenuConstants.Topping, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> BatchSort([FromBody] BatchSortRequest request)
  {
    var result = await _service.BatchSortToppings(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 400);
    return ApiOk();
  }

  [HttpDelete("batch")]
  [RequireRole(MenuConstants.Topping, CrudAction.Delete)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> BatchDelete([FromBody] BatchDeleteRequest request)
  {
    var result = await _service.BatchDeleteToppings(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 400, result.Errors);
    return ApiOk();
  }
}
