using Drink.Application.Requests.User;
using Drink.Application.Responses;
using Drink.Application.Responses.User;
using Drink.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drink.User.API.Controllers;

public class AuthController : BaseController
{
  private readonly UserAuthService _authService;

  public AuthController(UserAuthService authService)
  {
    _authService = authService;
  }

  /// <summary>
  /// 註冊（送出後會寄出 Email 驗證信）
  /// </summary>
  [HttpPost("register")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
  {
    var result = await _authService.Register(request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "EMAIL_ALREADY_EXISTS" ? 409 : 400;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return ApiOk();
  }

  /// <summary>
  /// 驗證 Email
  /// </summary>
  [HttpPost("verify-email")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> VerifyEmail([FromBody] UserVerifyEmailRequest request)
  {
    var result = await _authService.VerifyEmail(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!);
    return ApiOk();
  }

  /// <summary>
  /// 登入
  /// </summary>
  [HttpPost("login")]
  [ProducesResponseType(typeof(ApiResponse<UserLoginResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 401)]
  [ProducesResponseType(typeof(ApiResponse), 403)]
  public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
  {
    var result = await _authService.Login(request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error switch
      {
        "INVALID_CREDENTIALS" => 401,
        "ACCOUNT_INACTIVE" => 403,
        "EMAIL_NOT_VERIFIED" => 403,
        _ => 400
      };
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus);
    }
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 刷新 Token（Refresh Token Rotation）
  /// </summary>
  [HttpPost("refresh")]
  [ProducesResponseType(typeof(ApiResponse<UserLoginResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 403)]
  public async Task<IActionResult> Refresh([FromBody] UserRefreshTokenRequest request)
  {
    var result = await _authService.Refresh(request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "ACCOUNT_INACTIVE" ? 403 : 400;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus);
    }
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 登出（撤銷 refresh_token）
  /// </summary>
  [Authorize]
  [HttpPost("logout")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  public async Task<IActionResult> Logout([FromBody] UserLogoutRequest request)
  {
    await _authService.Logout(request);
    return ApiOk();
  }

  /// <summary>
  /// 變更密碼（成功後撤銷該用戶所有 refresh token）
  /// </summary>
  [Authorize]
  [HttpPut("password")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 401)]
  public async Task<IActionResult> ChangePassword([FromBody] ChangeUserPasswordRequest request)
  {
    var result = await _authService.ChangePassword(request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "UNAUTHORIZED" ? 401 : 400;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return ApiOk();
  }

  /// <summary>
  /// 登出所有裝置（撤銷該用戶所有 refresh token）
  /// </summary>
  [Authorize]
  [HttpPost("logout-all")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 401)]
  public async Task<IActionResult> LogoutAll()
  {
    await _authService.LogoutAll();
    return ApiOk();
  }
}
