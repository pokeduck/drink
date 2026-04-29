using Drink.Application.Responses.User;

namespace Drink.Application.Mappings;

public static class UserAuthMapper
{
  public static UserLoginResponse ToUserLoginResponse(string accessToken, string refreshToken)
    => new()
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken
    };
}
