using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Application.Services;
using Drink.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drink.Admin.API.Controllers;

[Authorize]
public class UsersController : BaseController
{
  private readonly AdminUserService _userService;

  public UsersController(AdminUserService userService)
  {
    _userService = userService;
  }

  /// <summary>
  /// 帳號列表
  /// </summary>
  [HttpGet]
  [ProducesResponseType(typeof(ApiResponse<PaginationExtension.PaginationList<AdminUserListResponse>>), 200)]
  public async Task<IActionResult> GetList(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? sortBy = null,
    [FromQuery] string? sortOrder = null,
    [FromQuery] string? keyword = null,
    [FromQuery] bool? isActive = null,
    [FromQuery] int? roleId = null)
  {
    var result = await _userService.GetList(page, pageSize, sortBy, sortOrder, keyword, isActive, roleId);
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 取得單一帳號
  /// </summary>
  [HttpGet("{userId}")]
  [ProducesResponseType(typeof(ApiResponse<AdminUserDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> GetById(int userId)
  {
    var result = await _userService.GetById(userId);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 建立 Staff
  /// </summary>
  [HttpPost]
  [ProducesResponseType(typeof(ApiResponse<AdminUserDetailResponse>), 201)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> Create([FromBody] CreateAdminUserRequest request)
  {
    var result = await _userService.Create(request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "USERNAME_ALREADY_EXISTS" ? 409 : 400;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return StatusCode(201, ApiResponse<AdminUserDetailResponse>.Success(result.Data));
  }

  /// <summary>
  /// 更新 Staff
  /// </summary>
  [HttpPut("{userId}")]
  [ProducesResponseType(typeof(ApiResponse<AdminUserDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 403)]
  public async Task<IActionResult> Update(int userId, [FromBody] UpdateAdminUserRequest request)
  {
    var result = await _userService.Update(userId, request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "CANNOT_CHANGE_ADMIN_ROLE" ? 403 : 400;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return ApiOk(result.Data);
  }

  /// <summary>
  /// Admin 重設 Staff 密碼
  /// </summary>
  [HttpPut("{userId}/password")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> ResetPassword(int userId, [FromBody] ResetAdminUserPasswordRequest request)
  {
    var result = await _userService.ResetPassword(userId, request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk();
  }

  /// <summary>
  /// 刪除 Staff
  /// </summary>
  [HttpDelete("{userId}")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 403)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> Delete(int userId)
  {
    var result = await _userService.Delete(userId);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "CANNOT_DELETE_ADMIN" ? 403 : 404;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus);
    }
    return ApiOk();
  }
}
