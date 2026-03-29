using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Drink.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Drink.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
  private readonly JwtSettings _settings;

  public JwtTokenService(IOptions<JwtSettings> settings)
  {
    _settings = settings.Value;
  }

  public string GenerateAccessToken(int userId, IEnumerable<Claim>? additionalClaims = null)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

    if (additionalClaims is not null)
      claims.AddRange(additionalClaims);

    var token = new JwtSecurityToken(
        issuer: _settings.Issuer,
        audience: _settings.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public string GenerateRefreshToken()
  {
    return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
  }

  public ClaimsPrincipal? ValidateToken(string token)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
    var handler = new JwtSecurityTokenHandler();

    try
    {
      return handler.ValidateToken(token, new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = _settings.Issuer,
        ValidAudience = _settings.Audience,
        IssuerSigningKey = key,
        ClockSkew = TimeSpan.Zero
      }, out _);
    }
    catch
    {
      return null;
    }
  }
}