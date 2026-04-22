using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Mappings;
using Drink.Application.Models;
using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Drink.Application.Services;

public class AdminUserService : BaseService
{
  private readonly IGenericRepository<AdminUser> _userRepo;
  private readonly IGenericRepository<AdminRole> _roleRepo;
  private readonly IGenericRepository<AdminRefreshToken> _tokenRepo;
  private readonly IPasswordHasher _passwordHasher;
  private readonly string _pepper;

  public AdminUserService(
    ICurrentUserContext currentUser,
    IGenericRepository<AdminUser> userRepo,
    IGenericRepository<AdminRole> roleRepo,
    IGenericRepository<AdminRefreshToken> tokenRepo,
    IPasswordHasher passwordHasher,
    IConfiguration configuration) : base(currentUser)
  {
    _userRepo = userRepo;
    _roleRepo = roleRepo;
    _tokenRepo = tokenRepo;
    _passwordHasher = passwordHasher;
    _pepper = configuration["Security:Pepper"]
              ?? throw new InvalidOperationException("Security:Pepper is not configured.");
  }

  public async Task<ApiResponse<PaginationList<AdminUserListResponse>>> GetList(
    int page, int pageSize, string? sortBy, string? sortOrder, string? keyword, bool? isActive, int? roleId)
  {
    var result = await _userRepo.GetPaginationList(
      page, pageSize,
      predicate: u =>
        (keyword == null || u.Username.Contains(keyword)) &&
        (isActive == null || u.IsActive == isActive) &&
        (roleId == null || u.RoleId == roleId),
      include: q => q.Include(u => u.Role),
      order: q => BuildOrder(q, sortBy, sortOrder));

    var mapped = new PaginationList<AdminUserListResponse>
    {
      Items = result.Items.ToAdminUserListResponseList(),
      Total = result.Total,
      Page = result.Page,
      PageSize = result.PageSize
    };

    return Success(mapped);
  }

  public async Task<ApiResponse<AdminUserDetailResponse>> GetById(int id)
  {
    var user = await _userRepo.GetById(id, include: q => q.Include(u => u.Role));

    if (user is null)
      return Fail<AdminUserDetailResponse>(ErrorCodes.NotFound, "帳號不存在");

    return Success(user.ToAdminUserDetailResponse());
  }

  public async Task<ApiResponse<AdminUserDetailResponse>> Create(CreateAdminUserRequest request)
  {
    if (await _userRepo.Any(u => u.Username == request.Username))
      return Fail<AdminUserDetailResponse>(
        ErrorCodes.UsernameAlreadyExists, "帳號已存在",
        new Dictionary<string, string[]> { ["username"] = ["帳號已存在"] });

    if (!await _roleRepo.Any(r => r.Id == request.RoleId))
      return Fail<AdminUserDetailResponse>(ErrorCodes.RoleNotFound, "角色不存在",
        new Dictionary<string, string[]> { ["role_id"] = ["角色不存在"] });

    var user = new AdminUser
    {
      Username = request.Username,
      PasswordHash = _passwordHasher.HashPassword(request.Password, _pepper),
      RoleId = request.RoleId,
      IsActive = request.IsActive
    };

    await _userRepo.Insert(user);

    // Reload with Role
    var created = (await _userRepo.GetById(user.Id, include: q => q.Include(u => u.Role)))!;
    return Success(created.ToAdminUserDetailResponse());
  }

  public async Task<ApiResponse<AdminUserDetailResponse>> Update(int id, UpdateAdminUserRequest request)
  {
    var user = await _userRepo.GetById(id, include: q => q.Include(u => u.Role), tracking: true);
    if (user is null)
      return Fail<AdminUserDetailResponse>(ErrorCodes.NotFound, "帳號不存在");

    // admin 帳號的 Role 不可改為非 Admin Role
    if (user.Role.IsSystem && !await IsSystemRole(request.RoleId))
      return Fail<AdminUserDetailResponse>(ErrorCodes.CannotChangeAdminRole, "不可將 Admin 帳號的角色改為非 Admin",
        new Dictionary<string, string[]> { ["role_id"] = ["不可將 Admin 帳號的角色改為非 Admin"] });

    if (!await _roleRepo.Any(r => r.Id == request.RoleId))
      return Fail<AdminUserDetailResponse>(ErrorCodes.RoleNotFound, "角色不存在",
        new Dictionary<string, string[]> { ["role_id"] = ["角色不存在"] });

    user.RoleId = request.RoleId;
    user.IsActive = request.IsActive;
    await _userRepo.Update(user);

    // Reload
    var updated = (await _userRepo.GetById(user.Id, include: q => q.Include(u => u.Role)))!;
    return Success(updated.ToAdminUserDetailResponse());
  }

  public async Task<ApiResponse> ResetPassword(int id, ResetAdminUserPasswordRequest request)
  {
    var user = await _userRepo.GetById(id, tracking: true);

    if (user is null)
      return Fail(ErrorCodes.NotFound, "帳號不存在");

    user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword, _pepper);
    await _userRepo.Update(user);

    // 撤銷該用戶所有 refresh_token
    await _tokenRepo.ExecuteUpdate(
      t => t.UserId == id && t.RevokedAt == null,
      setters => setters.SetProperty(t => t.RevokedAt, DateTime.UtcNow));

    return Success();
  }

  public async Task<ApiResponse> Delete(int id)
  {
    var user = await _userRepo.GetById(id, include: q => q.Include(u => u.Role));

    if (user is null)
      return Fail(ErrorCodes.NotFound, "帳號不存在");

    // admin 帳號不可刪除（判斷 Role.IsSystem + 是否為唯一 Admin）
    if (user.Role.IsSystem)
      return Fail(ErrorCodes.CannotDeleteAdmin, "不可刪除 Admin 帳號");

    // 撤銷所有 refresh_token
    await _tokenRepo.ExecuteUpdate(
      t => t.UserId == id && t.RevokedAt == null,
      setters => setters.SetProperty(t => t.RevokedAt, DateTime.UtcNow));

    await _userRepo.DeleteById(id);

    return Success();
  }

  private async Task<bool> IsSystemRole(int roleId)
  {
    return await _roleRepo.Any(r => r.Id == roleId && r.IsSystem);
  }

  private static IQueryable<AdminUser> BuildOrder(IQueryable<AdminUser> query, string? sortBy, string? sortOrder)
  {
    var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

    var ordered = sortBy?.ToLower() switch
    {
      "id" => isDesc ? query.OrderByDescending(u => u.Id) : query.OrderBy(u => u.Id),
      "username" => isDesc ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
      "is_active" => isDesc ? query.OrderByDescending(u => u.IsActive) : query.OrderBy(u => u.IsActive),
      "role_id" => isDesc ? query.OrderByDescending(u => u.RoleId) : query.OrderBy(u => u.RoleId),
      _ => isDesc ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
    };
    return ordered.ThenBy(u => u.Id);
  }
}
