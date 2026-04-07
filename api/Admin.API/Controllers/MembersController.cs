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
public class MembersController : BaseController
{
  private readonly MemberService _memberService;

  public MembersController(MemberService memberService)
  {
    _memberService = memberService;
  }

  /// <summary>
  /// 會員列表
  /// </summary>
  [HttpGet]
  [ProducesResponseType(typeof(ApiResponse<PaginationExtension.PaginationList<MemberListResponse>>), 200)]
  public async Task<IActionResult> GetList(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? sortBy = null,
    [FromQuery] string? sortOrder = null,
    [FromQuery] string? keyword = null,
    [FromQuery] UserStatus? status = null,
    [FromQuery] bool? emailVerified = null,
    [FromQuery] bool? isGoogleConnected = null)
  {
    var result = await _memberService.GetList(page, pageSize, sortBy, sortOrder, keyword, status, emailVerified, isGoogleConnected);
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 取得單一會員
  /// </summary>
  [HttpGet("{memberId}")]
  [ProducesResponseType(typeof(ApiResponse<MemberDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> GetById(int memberId)
  {
    var result = await _memberService.GetById(memberId);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 建立會員
  /// </summary>
  [HttpPost]
  [ProducesResponseType(typeof(ApiResponse<MemberDetailResponse>), 201)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
  {
    var result = await _memberService.Create(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 409, result.Errors);
    return StatusCode(201, ApiResponse<MemberDetailResponse>.Success(result.Data));
  }

  /// <summary>
  /// 編輯會員
  /// </summary>
  [HttpPut("{memberId}")]
  [ProducesResponseType(typeof(ApiResponse<MemberDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> Update(int memberId, [FromBody] UpdateMemberRequest request)
  {
    var result = await _memberService.Update(memberId, request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 重設會員密碼
  /// </summary>
  [HttpPut("{memberId}/password")]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> ResetPassword(int memberId, [FromBody] ResetMemberPasswordRequest request)
  {
    var result = await _memberService.ResetPassword(memberId, request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk();
  }
}
