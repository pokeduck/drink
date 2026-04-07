using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Application.Services;
using Drink.Domain.Enums;
using Drink.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drink.Admin.API.Controllers;

[Authorize]
public class VerificationsController : BaseController
{
  private readonly VerificationService _verificationService;

  public VerificationsController(VerificationService verificationService)
  {
    _verificationService = verificationService;
  }

  /// <summary>
  /// 註冊驗證信列表
  /// </summary>
  [HttpGet("register")]
  [ProducesResponseType(typeof(ApiResponse<PaginationExtension.PaginationList<VerificationListResponse>>), 200)]
  public async Task<IActionResult> GetRegisterList(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? sortBy = null,
    [FromQuery] string? sortOrder = null,
    [FromQuery] string? keyword = null,
    [FromQuery] bool? isSuccess = null,
    [FromQuery] bool? isUsed = null)
  {
    var result = await _verificationService.GetList(
      VerificationEmailType.Register, page, pageSize, sortBy, sortOrder, keyword, isSuccess, isUsed);
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 忘記密碼驗證信列表
  /// </summary>
  [HttpGet("forgot-password")]
  [ProducesResponseType(typeof(ApiResponse<PaginationExtension.PaginationList<VerificationListResponse>>), 200)]
  public async Task<IActionResult> GetForgotPasswordList(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? sortBy = null,
    [FromQuery] string? sortOrder = null,
    [FromQuery] string? keyword = null,
    [FromQuery] bool? isSuccess = null,
    [FromQuery] bool? isUsed = null)
  {
    var result = await _verificationService.GetList(
      VerificationEmailType.ForgotPassword, page, pageSize, sortBy, sortOrder, keyword, isSuccess, isUsed);
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 重發單筆驗證信
  /// </summary>
  [HttpPost("{verificationId}/resend")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  [ProducesResponseType(typeof(ApiResponse), 429)]
  public async Task<IActionResult> Resend(int verificationId)
  {
    var result = await _verificationService.Resend(verificationId);
    if (result.Code != 0)
    {
      var httpStatus = result.Error switch
      {
        "RESEND_TOO_FREQUENT" => 429,
        "NOT_FOUND" => 404,
        _ => 400
      };
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus);
    }
    return ApiOk();
  }

  /// <summary>
  /// 批量重發註冊驗證信
  /// </summary>
  [HttpPost("register/resend")]
  [ProducesResponseType(typeof(ApiResponse<BatchResendResponse>), 200)]
  public async Task<IActionResult> BatchResendRegister([FromBody] BatchResendRequest request)
  {
    var result = await _verificationService.BatchResend(VerificationEmailType.Register, request);
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 批量重發忘記密碼驗證信
  /// </summary>
  [HttpPost("forgot-password/resend")]
  [ProducesResponseType(typeof(ApiResponse<BatchResendResponse>), 200)]
  public async Task<IActionResult> BatchResendForgotPassword([FromBody] BatchResendRequest request)
  {
    var result = await _verificationService.BatchResend(VerificationEmailType.ForgotPassword, request);
    return ApiOk(result.Data);
  }
}
