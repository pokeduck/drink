using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Mappings;
using Drink.Application.Requests.User;
using Drink.Application.Responses;
using Drink.Application.Responses.User;
using Drink.Domain.Entities;

namespace Drink.Application.Services;

public class UserProfileService : BaseService
{
  private readonly IGenericRepository<User> _userRepo;

  public UserProfileService(
    ICurrentUserContext currentUser,
    IGenericRepository<User> userRepo) : base(currentUser)
  {
    _userRepo = userRepo;
  }

  public async Task<ApiResponse<UserProfileResponse>> GetProfile()
  {
    var user = await _userRepo.GetById(CurrentUserId);
    if (user is null)
      return Fail<UserProfileResponse>(ErrorCodes.Unauthorized, "使用者不存在");

    return Success(user.ToUserProfileResponse());
  }

  public async Task<ApiResponse<UserProfileResponse>> UpdateProfile(UpdateUserProfileRequest request)
  {
    var user = await _userRepo.GetById(CurrentUserId, tracking: true);
    if (user is null)
      return Fail<UserProfileResponse>(ErrorCodes.Unauthorized, "使用者不存在");

    user.ApplyUpdate(request);
    await _userRepo.Update(user);

    return Success(user.ToUserProfileResponse());
  }
}
