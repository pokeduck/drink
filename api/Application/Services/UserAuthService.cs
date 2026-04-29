using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Mappings;
using Drink.Application.Requests.User;
using Drink.Application.Responses;
using Drink.Application.Responses.User;
using Drink.Application.Settings;
using Drink.Domain.Entities;
using Drink.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Drink.Application.Services;

public class UserAuthService : BaseService
{
  private readonly IGenericRepository<User> _userRepo;
  private readonly IGenericRepository<UserRefreshToken> _tokenRepo;
  private readonly IGenericRepository<VerificationEmail> _verificationRepo;
  private readonly IJwtTokenService _jwtTokenService;
  private readonly JwtSettings _jwtSettings;
  private readonly IPasswordHasher _passwordHasher;
  private readonly VerificationService _verificationService;
  private readonly string _pepper;

  public UserAuthService(
    ICurrentUserContext currentUser,
    IGenericRepository<User> userRepo,
    IGenericRepository<UserRefreshToken> tokenRepo,
    IGenericRepository<VerificationEmail> verificationRepo,
    IJwtTokenService jwtTokenService,
    IOptions<JwtSettings> jwtSettings,
    IPasswordHasher passwordHasher,
    VerificationService verificationService,
    IConfiguration configuration) : base(currentUser)
  {
    _userRepo = userRepo;
    _tokenRepo = tokenRepo;
    _verificationRepo = verificationRepo;
    _jwtTokenService = jwtTokenService;
    _jwtSettings = jwtSettings.Value;
    _passwordHasher = passwordHasher;
    _verificationService = verificationService;
    _pepper = configuration["Security:Pepper"]
              ?? throw new InvalidOperationException("Security:Pepper is not configured.");
  }

  public async Task<ApiResponse> Register(UserRegisterRequest request)
  {
    var normalizedEmail = request.Email.Trim().ToLowerInvariant();

    var existing = await _userRepo.Get(u => u.Email.ToLower() == normalizedEmail);
    if (existing is not null)
      return Fail(ErrorCodes.EmailAlreadyExists, "Email 已被註冊",
        new Dictionary<string, string[]> { ["email"] = ["Email 已被註冊"] });

    var user = new User
    {
      Name = request.Name,
      Email = normalizedEmail,
      PasswordHash = _passwordHasher.HashPassword(request.Password, _pepper),
      NotificationType = NotificationType.None,
      Status = UserStatus.Active,
      EmailVerified = false,
      IsGoogleConnected = false
    };

    await _userRepo.Insert(user);

    await _verificationService.CreateAndSendVerification(user, VerificationEmailType.Register);

    return Success();
  }

  public async Task<ApiResponse> VerifyEmail(UserVerifyEmailRequest request)
  {
    var verification = await _verificationRepo.Get(
      v => v.Token == request.Token && v.Type == VerificationEmailType.Register,
      tracking: true);

    if (verification is null || verification.ExpiresAt < DateTime.UtcNow)
      return Fail(ErrorCodes.InvalidToken, "驗證連結無效或已過期");

    if (verification.IsUsed)
      return Fail(ErrorCodes.TokenAlreadyUsed, "此驗證連結已使用");

    var user = await _userRepo.GetById(verification.UserId, tracking: true);
    if (user is null)
      return Fail(ErrorCodes.InvalidToken, "驗證連結無效或已過期");

    user.EmailVerified = true;
    await _userRepo.Update(user);

    verification.IsUsed = true;
    verification.UsedAt = DateTime.UtcNow;
    await _verificationRepo.Update(verification);

    return Success();
  }

  public async Task<ApiResponse<UserLoginResponse>> Login(UserLoginRequest request)
  {
    var normalizedEmail = request.Email.Trim().ToLowerInvariant();
    var user = await _userRepo.Get(u => u.Email.ToLower() == normalizedEmail, tracking: true);

    if (user is null
        || string.IsNullOrEmpty(user.PasswordHash)
        || !_passwordHasher.VerifyPassword(request.Password, _pepper, user.PasswordHash))
      return Fail<UserLoginResponse>(ErrorCodes.UserInvalidCredentials, "Email 或密碼錯誤");

    if (user.Status == UserStatus.Inactive)
      return Fail<UserLoginResponse>(ErrorCodes.AccountInactive, "帳號已停用");

    if (!user.EmailVerified)
      return Fail<UserLoginResponse>(ErrorCodes.EmailNotVerified, "Email 尚未驗證");

    user.LastLoginAt = DateTime.UtcNow;
    await _userRepo.Update(user);

    var tokens = await GenerateAndSaveTokens(user.Id);
    return Success(tokens);
  }

  public async Task<ApiResponse<UserLoginResponse>> Refresh(UserRefreshTokenRequest request)
  {
    var existingToken = await _tokenRepo.Get(
      t => t.Token == request.RefreshToken,
      tracking: true);

    if (existingToken is null || existingToken.ExpiresAt < DateTime.UtcNow)
      return Fail<UserLoginResponse>(ErrorCodes.InvalidToken, "Token 無效或已過期");

    if (existingToken.RevokedAt is not null)
    {
      await RevokeAllTokens(existingToken.UserId);
      return Fail<UserLoginResponse>(ErrorCodes.InvalidToken, "Token 已被使用，已撤銷所有 Token");
    }

    existingToken.RevokedAt = DateTime.UtcNow;
    await _tokenRepo.Update(existingToken);

    var user = await _userRepo.GetById(existingToken.UserId);
    if (user is null || user.Status == UserStatus.Inactive)
      return Fail<UserLoginResponse>(ErrorCodes.AccountInactive, "帳號已停用");

    var tokens = await GenerateAndSaveTokens(existingToken.UserId);
    return Success(tokens);
  }

  public async Task<ApiResponse> Logout(UserLogoutRequest request)
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

  public async Task<ApiResponse> ChangePassword(ChangeUserPasswordRequest request)
  {
    var user = await _userRepo.GetById(CurrentUserId, tracking: true);
    if (user is null)
      return Fail(ErrorCodes.Unauthorized, "使用者不存在");

    if (string.IsNullOrEmpty(user.PasswordHash)
        || !_passwordHasher.VerifyPassword(request.OldPassword, _pepper, user.PasswordHash))
      return Fail(ErrorCodes.InvalidPassword, "舊密碼錯誤",
        new Dictionary<string, string[]> { ["old_password"] = ["舊密碼錯誤"] });

    user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword, _pepper);
    await _userRepo.Update(user);

    return Success();
  }

  public async Task<ApiResponse> LogoutAll()
  {
    await RevokeAllTokens(CurrentUserId);
    return Success();
  }

  private async Task<UserLoginResponse> GenerateAndSaveTokens(int userId)
  {
    var accessToken = _jwtTokenService.GenerateAccessToken(userId);
    var refreshToken = _jwtTokenService.GenerateRefreshToken();

    await _tokenRepo.Insert(new UserRefreshToken
    {
      UserId = userId,
      Token = refreshToken,
      ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
    });

    return UserAuthMapper.ToUserLoginResponse(accessToken, refreshToken);
  }

  private async Task RevokeAllTokens(int userId)
  {
    await _tokenRepo.ExecuteUpdate(
      t => t.UserId == userId && t.RevokedAt == null,
      setters => setters.SetProperty(t => t.RevokedAt, DateTime.UtcNow));
  }
}
