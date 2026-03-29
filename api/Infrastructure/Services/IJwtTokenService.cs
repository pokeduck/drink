using System.Security.Claims;

namespace Drink.Infrastructure.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(int userId, IEnumerable<Claim>? additionalClaims = null);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
