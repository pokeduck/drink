using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Application.Services;
using Drink.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drink.Admin.API.Controllers;

[Authorize]
public class SugarsController : BaseController
{
  private readonly DrinkOptionService _service;

  public SugarsController(DrinkOptionService service)
  {
    _service = service;
  }

  [HttpGet]
  [ProducesResponseType(typeof(ApiResponse<PaginationExtension.PaginationList<SugarListResponse>>), 200)]
  public async Task<IActionResult> GetList(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? sortBy = null,
    [FromQuery] string? sortOrder = null,
    [FromQuery] string? keyword = null)
  {
    var result = await _service.GetSugarList(page, pageSize, sortBy, sortOrder, keyword);
    return ApiOk(result.Data);
  }

  [HttpGet("{id}")]
  [ProducesResponseType(typeof(ApiResponse<SugarDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> GetById(int id)
  {
    var result = await _service.GetSugarById(id);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk(result.Data);
  }

  [HttpPost]
  [ProducesResponseType(typeof(ApiResponse<SugarDetailResponse>), 201)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> Create([FromBody] CreateSugarRequest request)
  {
    var result = await _service.CreateSugar(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 409, result.Errors);
    return StatusCode(201, ApiResponse<SugarDetailResponse>.Success(result.Data));
  }

  [HttpPut("{id}")]
  [ProducesResponseType(typeof(ApiResponse<SugarDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> Update(int id, [FromBody] UpdateSugarRequest request)
  {
    var result = await _service.UpdateSugar(id, request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "SUGAR_ALREADY_EXISTS" ? 409 : 404;
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
    var result = await _service.DeleteSugar(id);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "SUGAR_IN_USE" ? 400 : 404;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return ApiOk();
  }

  [HttpPut("sort")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> BatchSort([FromBody] BatchSortRequest request)
  {
    var result = await _service.BatchSortSugars(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 400);
    return ApiOk();
  }

  [HttpDelete("batch")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> BatchDelete([FromBody] BatchDeleteRequest request)
  {
    var result = await _service.BatchDeleteSugars(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 400, result.Errors);
    return ApiOk();
  }
}
