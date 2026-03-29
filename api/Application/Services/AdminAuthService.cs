using Drink.Application.Constants;
using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Drink.Infrastructure.Helpers;
using Drink.Infrastructure.Services;
using Drink.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Drink.Application.Services;

public class AdminAuthService : BaseService
{
  private readonly IJwtTokenService _jwtTokenService;
  private readonly JwtSettings _jwtSettings;
  private readonly string _pepper;

  public AdminAuthService(IServiceProvider serviceProvider) : base(serviceProvider)
  {
    _jwtTokenService = serviceProvider.GetRequiredService<IJwtTokenService>();
    _jwtSettings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
    _pepper = serviceProvider.GetRequiredService<IConfiguration>()["Security:Pepper"]
              ?? throw new InvalidOperationException("Security:Pepper is not configured.");
  }

  public async Task<ApiResponse<AdminLoginResponse>> Login(AdminLoginRequest request)
  {
    var userRepo = GetRepository<AdminUser>();
    var user = await userRepo.Get(u => u.Username == request.Username);

    if (user is null || !HashHelper.VerifyPassword(request.Password, _pepper, user.PasswordHash))
      return Fail<AdminLoginResponse>(ErrorCodes.InvalidCredentials, "帳號或密碼錯誤");

    if (!user.IsActive)
      return Fail<AdminLoginResponse>(ErrorCodes.AdminAccountInactive, "帳號已停用");

    var tokens = await GenerateAndSaveTokens(user.Id);
    return Success(tokens);
  }

  public async Task<ApiResponse<AdminLoginResponse>> Refresh(AdminRefreshTokenRequest request)
  {
    var tokenRepo = GetRepository<AdminRefreshToken>();

    var existingToken = await tokenRepo.Get(
      t => t.Token == request.RefreshToken,
      tracking: true);

    if (existingToken is null || existingToken.ExpiresAt < DateTime.UtcNow)
      return Fail<AdminLoginResponse>(ErrorCodes.InvalidToken, "Token 無效或已過期");

    // 重複使用偵測：已被撤銷的 token 再次使用，撤銷該用戶所有 token
    if (existingToken.RevokedAt is not null)
    {
      await RevokeAllTokens(existingToken.UserId);
      return Fail<AdminLoginResponse>(ErrorCodes.InvalidToken, "Token 已被使用，已撤銷所有 Token");
    }

    // 撤銷舊 token
    existingToken.RevokedAt = DateTime.UtcNow;
    await tokenRepo.Update(existingToken);

    // 確認用戶仍然有效
    var userRepo = GetRepository<AdminUser>();
    var user = await userRepo.GetById(existingToken.UserId);
    if (user is null || !user.IsActive)
      return Fail<AdminLoginResponse>(ErrorCodes.AdminAccountInactive, "帳號已停用");

    var tokens = await GenerateAndSaveTokens(existingToken.UserId);
    return Success(tokens);
  }

  public async Task<ApiResponse> Logout(AdminLogoutRequest request)
  {
    var tokenRepo = GetRepository<AdminRefreshToken>();
    var token = await tokenRepo.Get(
      t => t.Token == request.RefreshToken && t.RevokedAt == null,
      tracking: true);

    if (token is not null)
    {
      token.RevokedAt = DateTime.UtcNow;
      await tokenRepo.Update(token);
    }

    return Success();
  }

  public async Task<ApiResponse> ChangePassword(AdminChangePasswordRequest request)
  {
    var userRepo = GetRepository<AdminUser>();
    var user = await userRepo.GetById(CurrentUserId, tracking: true);

    if (user is null)
      return Fail(ErrorCodes.Unauthorized, "使用者不存在");

    if (!HashHelper.VerifyPassword(request.OldPassword, _pepper, user.PasswordHash))
      return Fail(ErrorCodes.InvalidPassword, "舊密碼錯誤");

    user.PasswordHash = HashHelper.HashPassword(request.NewPassword, _pepper);
    await userRepo.Update(user);

    await RevokeAllTokens(user.Id);

    return Success();
  }

  private async Task<AdminLoginResponse> GenerateAndSaveTokens(int userId)
  {
    var accessToken = _jwtTokenService.GenerateAccessToken(userId);
    var refreshToken = _jwtTokenService.GenerateRefreshToken();

    var tokenRepo = GetRepository<AdminRefreshToken>();
    await tokenRepo.Insert(new AdminRefreshToken
    {
      UserId = userId,
      Token = refreshToken,
      ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
    });

    return new AdminLoginResponse
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken
    };
  }

  private async Task RevokeAllTokens(int userId)
  {
    var tokenRepo = GetRepository<AdminRefreshToken>();
    await tokenRepo.ExecuteUpdate(
      t => t.UserId == userId && t.RevokedAt == null,
      setters => setters.SetProperty(t => t.RevokedAt, DateTime.UtcNow));
  }
}
