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
public class SizesController : BaseController
{
  private readonly DrinkOptionService _service;

  public SizesController(DrinkOptionService service)
  {
    _service = service;
  }

  [HttpGet]
  [RequireRole(MenuConstants.Size, CrudAction.Read)]
  [ProducesResponseType(typeof(ApiResponse<PaginationList<SizeListResponse>>), 200)]
  public async Task<IActionResult> GetList(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? sortBy = null,
    [FromQuery] string? sortOrder = null,
    [FromQuery] string? keyword = null)
  {
    var result = await _service.GetSizeList(page, pageSize, sortBy, sortOrder, keyword);
    return ApiOk(result.Data);
  }

  [HttpGet("{id}")]
  [RequireRole(MenuConstants.Size, CrudAction.Read)]
  [ProducesResponseType(typeof(ApiResponse<SizeDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> GetById(int id)
  {
    var result = await _service.GetSizeById(id);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk(result.Data);
  }

  [HttpPost]
  [RequireRole(MenuConstants.Size, CrudAction.Create)]
  [ProducesResponseType(typeof(ApiResponse<SizeDetailResponse>), 201)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> Create([FromBody] CreateSizeRequest request)
  {
    var result = await _service.CreateSize(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 409, result.Errors);
    return StatusCode(201, ApiResponse<SizeDetailResponse>.Success(result.Data));
  }

  [HttpPut("{id}")]
  [RequireRole(MenuConstants.Size, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse<SizeDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> Update(int id, [FromBody] UpdateSizeRequest request)
  {
    var result = await _service.UpdateSize(id, request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "SIZE_ALREADY_EXISTS" ? 409 : 404;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return ApiOk(result.Data);
  }

  [HttpDelete("{id}")]
  [RequireRole(MenuConstants.Size, CrudAction.Delete)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> Delete(int id)
  {
    var result = await _service.DeleteSize(id);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "SIZE_IN_USE" ? 400 : 404;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return ApiOk();
  }

  [HttpPut("sort")]
  [RequireRole(MenuConstants.Size, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> BatchSort([FromBody] BatchSortRequest request)
  {
    var result = await _service.BatchSortSizes(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 400);
    return ApiOk();
  }

  [HttpDelete("batch")]
  [RequireRole(MenuConstants.Size, CrudAction.Delete)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> BatchDelete([FromBody] BatchDeleteRequest request)
  {
    var result = await _service.BatchDeleteSizes(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 400, result.Errors);
    return ApiOk();
  }
}
