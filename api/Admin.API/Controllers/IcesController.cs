using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Application.Services;
using Drink.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drink.Admin.API.Controllers;

[Authorize]
public class IcesController : BaseController
{
  private readonly DrinkOptionService _service;

  public IcesController(DrinkOptionService service)
  {
    _service = service;
  }

  [HttpGet]
  [ProducesResponseType(typeof(ApiResponse<PaginationList<IceListResponse>>), 200)]
  public async Task<IActionResult> GetList(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? sortBy = null,
    [FromQuery] string? sortOrder = null,
    [FromQuery] string? keyword = null)
  {
    var result = await _service.GetIceList(page, pageSize, sortBy, sortOrder, keyword);
    return ApiOk(result.Data);
  }

  [HttpGet("{id}")]
  [ProducesResponseType(typeof(ApiResponse<IceDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> GetById(int id)
  {
    var result = await _service.GetIceById(id);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk(result.Data);
  }

  [HttpPost]
  [ProducesResponseType(typeof(ApiResponse<IceDetailResponse>), 201)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> Create([FromBody] CreateIceRequest request)
  {
    var result = await _service.CreateIce(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 409, result.Errors);
    return StatusCode(201, ApiResponse<IceDetailResponse>.Success(result.Data));
  }

  [HttpPut("{id}")]
  [ProducesResponseType(typeof(ApiResponse<IceDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> Update(int id, [FromBody] UpdateIceRequest request)
  {
    var result = await _service.UpdateIce(id, request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "ICE_ALREADY_EXISTS" ? 409 : 404;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return ApiOk(result.Data);
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> Delete(int id)
  {
    var result = await _service.DeleteIce(id);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "ICE_IN_USE" ? 400 : 404;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return ApiOk();
  }

  [HttpPut("sort")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> BatchSort([FromBody] BatchSortRequest request)
  {
    var result = await _service.BatchSortIces(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 400);
    return ApiOk();
  }

  [HttpDelete("batch")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> BatchDelete([FromBody] BatchDeleteRequest request)
  {
    var result = await _service.BatchDeleteIces(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 400, result.Errors);
    return ApiOk();
  }
}
