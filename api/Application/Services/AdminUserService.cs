using Drink.Application.Constants;
using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Drink.Infrastructure.Extensions;
using Drink.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Drink.Application.Services;

public class AdminUserService : BaseService
{
  private readonly string _pepper;

  public AdminUserService(IServiceProvider serviceProvider) : base(serviceProvider)
  {
    _pepper = serviceProvider.GetRequiredService<IConfiguration>()["Security:Pepper"]
              ?? throw new InvalidOperationException("Security:Pepper is not configured.");
  }

  public async Task<ApiResponse<PaginationExtension.PaginationList<AdminUserListResponse>>> GetList(
    int page, int pageSize, string? sortBy, string? sortOrder, string? keyword, bool? isActive, int? roleId)
  {
    var repo = GetRepository<AdminUser>();

    var result = await repo.GetPaginationList(
      page, pageSize,
      predicate: u =>
        (keyword == null || u.Username.Contains(keyword)) &&
        (isActive == null || u.IsActive == isActive) &&
        (roleId == null || u.RoleId == roleId),
      include: q => q.Include(u => u.Role),
      order: q => BuildOrder(q, sortBy, sortOrder));

    var mapped = new PaginationExtension.PaginationList<AdminUserListResponse>
    {
      Items = Mapper.Map<List<AdminUserListResponse>>(result.Items),
      Total = result.Total,
      Page = result.Page,
      PageSize = result.PageSize
    };

    return Success(mapped);
  }

  public async Task<ApiResponse<AdminUserDetailResponse>> GetById(int id)
  {
    var repo = GetRepository<AdminUser>();
    var user = await repo.GetById(id, include: q => q.Include(u => u.Role));

    if (user is null)
      return Fail<AdminUserDetailResponse>(ErrorCodes.NotFound, "帳號不存在");

    return Success(Mapper.Map<AdminUserDetailResponse>(user));
  }

  public async Task<ApiResponse<AdminUserDetailResponse>> Create(CreateAdminUserRequest request)
  {
    var userRepo = GetRepository<AdminUser>();
    var roleRepo = GetRepository<AdminRole>();

    if (await userRepo.Any(u => u.Username == request.Username))
      return Fail<AdminUserDetailResponse>(
        ErrorCodes.UsernameAlreadyExists, "帳號已存在",
        new Dictionary<string, string[]> { ["username"] = ["帳號已存在"] });

    if (!await roleRepo.Any(r => r.Id == request.RoleId))
      return Fail<AdminUserDetailResponse>(ErrorCodes.RoleNotFound, "角色不存在");

    var user = new AdminUser
    {
      Username = request.Username,
      PasswordHash = HashHelper.HashPassword(request.Password, _pepper),
      RoleId = request.RoleId,
      IsActive = request.IsActive
    };

    await userRepo.Insert(user);

    // Reload with Role
    var created = await userRepo.GetById(user.Id, include: q => q.Include(u => u.Role));
    return Success(Mapper.Map<AdminUserDetailResponse>(created));
  }

  public async Task<ApiResponse<AdminUserDetailResponse>> Update(int id, UpdateAdminUserRequest request)
  {
    var userRepo = GetRepository<AdminUser>();
    var roleRepo = GetRepository<AdminRole>();

    var user = await userRepo.GetById(id, include: q => q.Include(u => u.Role), tracking: true);
    if (user is null)
      return Fail<AdminUserDetailResponse>(ErrorCodes.NotFound, "帳號不存在");

    // admin 帳號的 Role 不可改為非 Admin Role
    if (user.Role.IsSystem && !await IsSystemRole(request.RoleId))
      return Fail<AdminUserDetailResponse>(ErrorCodes.CannotChangeAdminRole, "不可將 Admin 帳號的角色改為非 Admin");

    if (!await roleRepo.Any(r => r.Id == request.RoleId))
      return Fail<AdminUserDetailResponse>(ErrorCodes.RoleNotFound, "角色不存在");

    user.RoleId = request.RoleId;
    user.IsActive = request.IsActive;
    await userRepo.Update(user);

    // Reload
    var updated = await userRepo.GetById(user.Id, include: q => q.Include(u => u.Role));
    return Success(Mapper.Map<AdminUserDetailResponse>(updated));
  }

  public async Task<ApiResponse> ResetPassword(int id, ResetAdminUserPasswordRequest request)
  {
    var userRepo = GetRepository<AdminUser>();
    var user = await userRepo.GetById(id, tracking: true);

    if (user is null)
      return Fail(ErrorCodes.NotFound, "帳號不存在");

    user.PasswordHash = HashHelper.HashPassword(request.NewPassword, _pepper);
    await userRepo.Update(user);

    // 撤銷該用戶所有 refresh_token
    var tokenRepo = GetRepository<AdminRefreshToken>();
    await tokenRepo.ExecuteUpdate(
      t => t.UserId == id && t.RevokedAt == null,
      setters => setters.SetProperty(t => t.RevokedAt, DateTime.UtcNow));

    return Success();
  }

  public async Task<ApiResponse> Delete(int id)
  {
    var userRepo = GetRepository<AdminUser>();
    var user = await userRepo.GetById(id, include: q => q.Include(u => u.Role));

    if (user is null)
      return Fail(ErrorCodes.NotFound, "帳號不存在");

    // admin 帳號不可刪除（判斷 Role.IsSystem + 是否為唯一 Admin）
    if (user.Role.IsSystem)
      return Fail(ErrorCodes.CannotDeleteAdmin, "不可刪除 Admin 帳號");

    // 撤銷所有 refresh_token
    var tokenRepo = GetRepository<AdminRefreshToken>();
    await tokenRepo.ExecuteUpdate(
      t => t.UserId == id && t.RevokedAt == null,
      setters => setters.SetProperty(t => t.RevokedAt, DateTime.UtcNow));

    await userRepo.DeleteById(id);

    return Success();
  }

  private async Task<bool> IsSystemRole(int roleId)
  {
    var roleRepo = GetRepository<AdminRole>();
    return await roleRepo.Any(r => r.Id == roleId && r.IsSystem);
  }

  private static IQueryable<AdminUser> BuildOrder(IQueryable<AdminUser> query, string? sortBy, string? sortOrder)
  {
    var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

    return sortBy?.ToLower() switch
    {
      "username" => isDesc ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
      "is_active" => isDesc ? query.OrderByDescending(u => u.IsActive) : query.OrderBy(u => u.IsActive),
      "role_id" => isDesc ? query.OrderByDescending(u => u.RoleId) : query.OrderBy(u => u.RoleId),
      _ => isDesc ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
    };
  }
}
