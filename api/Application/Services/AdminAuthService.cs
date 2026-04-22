using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Application.Settings;
using Drink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Drink.Application.Services;

public class AdminAuthService : BaseService
{
  private readonly IGenericRepository<AdminUser> _userRepo;
  private readonly IGenericRepository<AdminRefreshToken> _tokenRepo;
  private readonly IJwtTokenService _jwtTokenService;
  private readonly JwtSettings _jwtSettings;
  private readonly IPasswordHasher _passwordHasher;
  private readonly string _pepper;

  public AdminAuthService(
    ICurrentUserContext currentUser,
    IGenericRepository<AdminUser> userRepo,
    IGenericRepository<AdminRefreshToken> tokenRepo,
    IJwtTokenService jwtTokenService,
    IOptions<JwtSettings> jwtSettings,
    IPasswordHasher passwordHasher,
    IConfiguration configuration) : base(currentUser)
  {
    _userRepo = userRepo;
    _tokenRepo = tokenRepo;
    _jwtTokenService = jwtTokenService;
    _jwtSettings = jwtSettings.Value;
    _passwordHasher = passwordHasher;
    _pepper = configuration["Security:Pepper"]
              ?? throw new InvalidOperationException("Security:Pepper is not configured.");
  }

  public async Task<ApiResponse<AdminLoginResponse>> Login(AdminLoginRequest request)
  {
    var user = await _userRepo.Get(u => u.Username == request.Username);

    if (user is null || !_passwordHasher.VerifyPassword(request.Password, _pepper, user.PasswordHash))
      return Fail<AdminLoginResponse>(ErrorCodes.InvalidCredentials, "帳號或密碼錯誤");

    if (!user.IsActive)
      return Fail<AdminLoginResponse>(ErrorCodes.AdminAccountInactive, "帳號已停用");

    var tokens = await GenerateAndSaveTokens(user.Id);
    return Success(tokens);
  }

  public async Task<ApiResponse<AdminLoginResponse>> Refresh(AdminRefreshTokenRequest request)
  {
    var existingToken = await _tokenRepo.Get(
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
    await _tokenRepo.Update(existingToken);

    // 確認用戶仍然有效
    var user = await _userRepo.GetById(existingToken.UserId);
    if (user is null || !user.IsActive)
      return Fail<AdminLoginResponse>(ErrorCodes.AdminAccountInactive, "帳號已停用");

    var tokens = await GenerateAndSaveTokens(existingToken.UserId);
    return Success(tokens);
  }

  public async Task<ApiResponse> Logout(AdminLogoutRequest request)
  {
    var token = await _tokenRepo.Get(
      t => t.Token == request.RefreshToken && t.RevokedAt == null,
      tracking: true);

    if (token is not null)
    {
      token.RevokedAt = DateTime.UtcNow;
      await _tokenRepo.Update(token);
    }

    return Success();
  }

  public async Task<ApiResponse> ChangePassword(AdminChangePasswordRequest request)
  {
    var user = await _userRepo.GetById(CurrentUserId, tracking: true);

    if (user is null)
      return Fail(ErrorCodes.Unauthorized, "使用者不存在");

    if (!_passwordHasher.VerifyPassword(request.OldPassword, _pepper, user.PasswordHash))
      return Fail(ErrorCodes.InvalidPassword, "舊密碼錯誤",
        new Dictionary<string, string[]> { ["old_password"] = ["舊密碼錯誤"] });

    user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword, _pepper);
    await _userRepo.Update(user);

    await RevokeAllTokens(user.Id);

    return Success();
  }

  private async Task<AdminLoginResponse> GenerateAndSaveTokens(int userId)
  {
    var accessToken = _jwtTokenService.GenerateAccessToken(userId);
    var refreshToken = _jwtTokenService.GenerateRefreshToken();

    await _tokenRepo.Insert(new AdminRefreshToken
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
    await _tokenRepo.ExecuteUpdate(
      t => t.UserId == userId && t.RevokedAt == null,
      setters => setters.SetProperty(t => t.RevokedAt, DateTime.UtcNow));
  }
}
