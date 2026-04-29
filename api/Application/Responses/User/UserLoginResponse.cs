namespace Drink.Application.Responses.User;

public class UserLoginResponse
{
  public string AccessToken { get; set; } = null!;
  public string RefreshToken { get; set; } = null!;
}
