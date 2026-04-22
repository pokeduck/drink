using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Mappings;
using Drink.Application.Models;
using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Drink.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace Drink.Application.Services;

public class MemberService : BaseService
{
  private readonly IGenericRepository<User> _userRepo;
  private readonly IGenericRepository<UserRefreshToken> _tokenRepo;
  private readonly IPasswordHasher _passwordHasher;
  private readonly string _pepper;

  public MemberService(
    ICurrentUserContext currentUser,
    IGenericRepository<User> userRepo,
    IGenericRepository<UserRefreshToken> tokenRepo,
    IPasswordHasher passwordHasher,
    IConfiguration configuration) : base(currentUser)
  {
    _userRepo = userRepo;
    _tokenRepo = tokenRepo;
    _passwordHasher = passwordHasher;
    _pepper = configuration["Security:Pepper"]
              ?? throw new InvalidOperationException("Security:Pepper is not configured.");
  }

  public async Task<ApiResponse<PaginationList<MemberListResponse>>> GetList(
    int page, int pageSize, string? sortBy, string? sortOrder,
    string? keyword, UserStatus? status, bool? emailVerified, bool? isGoogleConnected)
  {
    var result = await _userRepo.GetPaginationList(
      page, pageSize,
      predicate: u =>
        (keyword == null || u.Name.Contains(keyword) || u.Email.Contains(keyword)) &&
        (status == null || u.Status == status) &&
        (emailVerified == null || u.EmailVerified == emailVerified) &&
        (isGoogleConnected == null || u.IsGoogleConnected == isGoogleConnected),
      order: q => BuildOrder(q, sortBy, sortOrder));

    var mapped = new PaginationList<MemberListResponse>
    {
      Items = result.Items.Select(u => u.ToMemberListResponse()).ToList(),
      Total = result.Total,
      Page = result.Page,
      PageSize = result.PageSize
    };

    return Success(mapped);
  }

  public async Task<ApiResponse<MemberDetailResponse>> GetById(int id)
  {
    var user = await _userRepo.GetById(id);

    if (user is null)
      return Fail<MemberDetailResponse>(ErrorCodes.NotFound, "會員不存在");

    return Success(user.ToMemberDetailResponse());
  }

  public async Task<ApiResponse<MemberDetailResponse>> Create(CreateMemberRequest request)
  {
    if (await _userRepo.Any(u => u.Email.ToLower() == request.Email.ToLower()))
      return Fail<MemberDetailResponse>(
        ErrorCodes.EmailAlreadyExists, "Email 已存在",
        new Dictionary<string, string[]> { ["email"] = ["Email 已存在"] });

    var user = new User
    {
      Name = request.Name,
      Email = request.Email,
      PasswordHash = _passwordHasher.HashPassword(request.Password, _pepper),
      NotificationType = NotificationType.None,
      Status = UserStatus.Active,
      EmailVerified = true,
      IsGoogleConnected = false
    };

    await _userRepo.Insert(user);

    var created = (await _userRepo.GetById(user.Id))!;
    return Success(created.ToMemberDetailResponse());
  }

  public async Task<ApiResponse<MemberDetailResponse>> Update(int id, UpdateMemberRequest request)
  {
    var user = await _userRepo.GetById(id, tracking: true);

    if (user is null)
      return Fail<MemberDetailResponse>(ErrorCodes.NotFound, "會員不存在");

    user.Name = request.Name;
    user.Avatar = request.Avatar;
    user.NotificationType = request.NotificationType;
    user.Status = request.Status;
    await _userRepo.Update(user);

    var updated = (await _userRepo.GetById(user.Id))!;
    return Success(updated.ToMemberDetailResponse());
  }

  public async Task<ApiResponse> ResetPassword(int id, ResetMemberPasswordRequest request)
  {
    var user = await _userRepo.GetById(id, tracking: true);

    if (user is null)
      return Fail(ErrorCodes.NotFound, "會員不存在");

    user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword, _pepper);
    await _userRepo.Update(user);

    // 撤銷該用戶所有 UserRefreshToken
    await _tokenRepo.ExecuteUpdate(
      t => t.UserId == id && t.RevokedAt == null,
      setters => setters.SetProperty(t => t.RevokedAt, DateTime.UtcNow));

    return Success();
  }

  private static IQueryable<User> BuildOrder(IQueryable<User> query, string? sortBy, string? sortOrder)
  {
    var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

    var ordered = sortBy?.ToLower() switch
    {
      "id" => isDesc ? query.OrderByDescending(u => u.Id) : query.OrderBy(u => u.Id),
      "name" => isDesc ? query.OrderByDescending(u => u.Name) : query.OrderBy(u => u.Name),
      "email" => isDesc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
      "status" => isDesc ? query.OrderByDescending(u => u.Status) : query.OrderBy(u => u.Status),
      _ => isDesc ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
    };
    return ordered.ThenBy(u => u.Id);
  }
}
