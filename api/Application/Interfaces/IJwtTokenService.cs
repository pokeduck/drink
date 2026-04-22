using System.Security.Claims;

namespace Drink.Application.Interfaces;

public interface IJwtTokenService
{
  string GenerateAccessToken(int userId, IEnumerable<Claim>? additionalClaims = null);
  string GenerateRefreshToken();
  ClaimsPrincipal? ValidateToken(string token);
}
