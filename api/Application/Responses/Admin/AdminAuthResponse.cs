namespace Drink.Application.Responses.Admin;

public class AdminLoginResponse
{
  public string AccessToken { get; set; } = null!;
  public string RefreshToken { get; set; } = null!;
}
