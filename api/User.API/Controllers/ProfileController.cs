using Drink.Application.Requests.User;
using Drink.Application.Responses;
using Drink.Application.Responses.User;
using Drink.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drink.User.API.Controllers;

[Authorize]
public class ProfileController : BaseController
{
  private readonly UserProfileService _profileService;

  public ProfileController(UserProfileService profileService)
  {
    _profileService = profileService;
  }

  /// <summary>
  /// 取得個人資料
  /// </summary>
  [HttpGet]
  [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 401)]
  public async Task<IActionResult> Get()
  {
    var result = await _profileService.GetProfile();
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 401);
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 更新個人資料（name / avatar / notification_type）
  /// </summary>
  [HttpPut]
  [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 401)]
  public async Task<IActionResult> Update([FromBody] UpdateUserProfileRequest request)
  {
    var result = await _profileService.UpdateProfile(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 401);
    return ApiOk(result.Data);
  }
}
