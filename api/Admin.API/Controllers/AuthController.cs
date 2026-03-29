using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drink.Admin.API.Controllers;

public class AuthController : BaseController
{
  private readonly AdminAuthService _authService;

  public AuthController(AdminAuthService authService)
  {
    _authService = authService;
  }

  /// <summary>
  /// 登入
  /// </summary>
  [HttpPost("login")]
  [ProducesResponseType(typeof(ApiResponse<AdminLoginResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 403)]
  public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
  {
    var result = await _authService.Login(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!,
        result.Error == "ADMIN_ACCOUNT_INACTIVE" ? 403 : 400);
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 刷新 Token
  /// </summary>
  [HttpPost("refresh")]
  [ProducesResponseType(typeof(ApiResponse<AdminLoginResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> Refresh([FromBody] AdminRefreshTokenRequest request)
  {
    var result = await _authService.Refresh(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!);
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 登出
  /// </summary>
  [Authorize]
  [HttpPost("logout")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  public async Task<IActionResult> Logout([FromBody] AdminLogoutRequest request)
  {
    var result = await _authService.Logout(request);
    return ApiOk();
  }

  /// <summary>
  /// 修改自己密碼
  /// </summary>
  [Authorize]
  [HttpPut("password")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> ChangePassword([FromBody] AdminChangePasswordRequest request)
  {
    var result = await _authService.ChangePassword(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!);
    return ApiOk();
  }
}
