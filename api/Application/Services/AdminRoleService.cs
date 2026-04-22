using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Mappings;
using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Drink.Application.Services;

public class AdminRoleService : BaseService
{
  private readonly IGenericRepository<AdminRole> _roleRepo;
  private readonly IGenericRepository<AdminMenu> _menuRepo;
  private readonly IGenericRepository<AdminMenuRole> _menuRoleRepo;
  private readonly IGenericRepository<AdminUser> _userRepo;

  public AdminRoleService(
    ICurrentUserContext currentUser,
    IGenericRepository<AdminRole> roleRepo,
    IGenericRepository<AdminMenu> menuRepo,
    IGenericRepository<AdminMenuRole> menuRoleRepo,
    IGenericRepository<AdminUser> userRepo) : base(currentUser)
  {
    _roleRepo = roleRepo;
    _menuRepo = menuRepo;
    _menuRoleRepo = menuRoleRepo;
    _userRepo = userRepo;
  }

  /// <summary>
  /// 取得所有 Role（含 StaffCount）
  /// </summary>
  public async Task<ApiResponse<List<AdminRoleListResponse>>> GetList()
  {
    var roles = await _roleRepo.GetList(
      include: q => q.Include(r => r.Users),
      order: q => q.OrderBy(r => r.Id));

    return Success(roles.ToAdminRoleListResponseList());
  }

  /// <summary>
  /// 取得單一 Role（含所有 Menu CRUD 設定）
  /// </summary>
  public async Task<ApiResponse<AdminRoleDetailResponse>> GetById(int roleId)
  {
    var role = await _roleRepo.GetById(roleId);

    if (role is null)
      return Fail<AdminRoleDetailResponse>(ErrorCodes.RoleNotFound, "角色不存在");

    // 取得所有葉節點 Menu（有 Endpoint 的）
    var leafMenus = await _menuRepo.GetList(
      predicate: m => m.Endpoint != null,
      order: q => q.OrderBy(m => m.Sort));

    // 取得該 Role 的所有 MenuRole
    var menuRoles = await _menuRoleRepo.GetList(
      predicate: mr => mr.RoleId == roleId);

    var menuRoleDict = menuRoles.ToDictionary(mr => mr.MenuId);

    var response = new AdminRoleDetailResponse
    {
      Id = role.Id,
      Name = role.Name,
      IsSystem = role.IsSystem,
      Menus = leafMenus.Select(m =>
      {
        menuRoleDict.TryGetValue(m.Id, out var mr);
        return new AdminMenuRoleResponse
        {
          MenuId = m.Id,
          MenuName = m.Name,
          CanRead = mr?.CanRead ?? false,
          CanCreate = mr?.CanCreate ?? false,
          CanUpdate = mr?.CanUpdate ?? false,
          CanDelete = mr?.CanDelete ?? false,
        };
      }).ToList()
    };

    return Success(response);
  }

  /// <summary>
  /// 建立 Role + Menu CRUD
  /// </summary>
  public async Task<ApiResponse<AdminRoleDetailResponse>> Create(CreateAdminRoleRequest request)
  {
    // 名稱唯一
    if (await _roleRepo.Any(r => r.Name == request.Name))
      return Fail<AdminRoleDetailResponse>(
        ErrorCodes.RoleAlreadyExists, "角色名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["角色名稱已存在"] });

    // 驗證所有 MenuId 合法（僅葉節點）
    if (!await ValidateMenuIds(request.Menus.Select(m => m.MenuId).ToList()))
      return Fail<AdminRoleDetailResponse>(ErrorCodes.InvalidMenuId, "包含無效的 Menu ID",
        new Dictionary<string, string[]> { ["menus"] = ["包含無效的 Menu ID"] });

    var role = new AdminRole
    {
      Name = request.Name,
      IsSystem = false,
    };

    await _roleRepo.Insert(role);

    // 建立 MenuRole
    await SaveMenuRoles(role.Id, request.Menus);

    return await GetById(role.Id);
  }

  /// <summary>
  /// 更新 Role + Menu CRUD（整批覆蓋）
  /// </summary>
  public async Task<ApiResponse<AdminRoleDetailResponse>> Update(int roleId, UpdateAdminRoleRequest request)
  {
    var role = await _roleRepo.GetById(roleId, tracking: true);

    if (role is null)
      return Fail<AdminRoleDetailResponse>(ErrorCodes.RoleNotFound, "角色不存在");

    if (role.IsSystem)
      return Fail<AdminRoleDetailResponse>(ErrorCodes.CannotModifySystemRole, "不可修改系統角色");

    // 名稱唯一（排除自己）
    if (await _roleRepo.Any(r => r.Name == request.Name && r.Id != roleId))
      return Fail<AdminRoleDetailResponse>(
        ErrorCodes.RoleAlreadyExists, "角色名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["角色名稱已存在"] });

    // 驗證所有 MenuId 合法
    if (!await ValidateMenuIds(request.Menus.Select(m => m.MenuId).ToList()))
      return Fail<AdminRoleDetailResponse>(ErrorCodes.InvalidMenuId, "包含無效的 Menu ID",
        new Dictionary<string, string[]> { ["menus"] = ["包含無效的 Menu ID"] });

    role.Name = request.Name;
    await _roleRepo.Update(role);

    // 整批覆蓋 MenuRole：先刪後建
    await _menuRoleRepo.ExecuteDelete(mr => mr.RoleId == roleId);
    await SaveMenuRoles(roleId, request.Menus);

    return await GetById(roleId);
  }

  /// <summary>
  /// 刪除 Role（有 Staff 時必須提供 reassignRoleId）
  /// </summary>
  public async Task<ApiResponse> Delete(int roleId, DeleteAdminRoleRequest? request)
  {
    var role = await _roleRepo.GetById(roleId, include: q => q.Include(r => r.Users));

    if (role is null)
      return Fail(ErrorCodes.RoleNotFound, "角色不存在");

    if (role.IsSystem)
      return Fail(ErrorCodes.CannotDeleteSystemRole, "不可刪除系統角色");

    var staffCount = role.Users.Count;

    if (staffCount > 0)
    {
      if (request?.ReassignRoleId is null)
        return Fail(ErrorCodes.RoleHasStaff, $"此角色下有 {staffCount} 個帳號，請指定遷移目標角色");

      var targetRoleId = request.ReassignRoleId.Value;

      // 不可遷移到自己
      if (targetRoleId == roleId)
        return Fail(ErrorCodes.RoleHasStaff, "不可遷移到相同角色");

      // 目標 Role 必須存在
      if (!await _roleRepo.Any(r => r.Id == targetRoleId))
        return Fail(ErrorCodes.RoleNotFound, "目標角色不存在");

      // 遷移 Staff
      await _userRepo.ExecuteUpdate(
        u => u.RoleId == roleId,
        setters => setters.SetProperty(u => u.RoleId, targetRoleId));
    }

    // 刪除 MenuRole
    await _menuRoleRepo.ExecuteDelete(mr => mr.RoleId == roleId);

    // 刪除 Role
    await _roleRepo.DeleteById(roleId);

    return Success();
  }

  private async Task<bool> ValidateMenuIds(List<int> menuIds)
  {
    if (menuIds.Count == 0) return true;

    var validCount = await _menuRepo.Count(m => m.Endpoint != null && menuIds.Contains(m.Id));
    return validCount == menuIds.Count;
  }

  private async Task SaveMenuRoles(int roleId, List<MenuCrudRequest> menus)
  {
    if (menus.Count == 0) return;

    var entities = menus.Select(m => new AdminMenuRole
    {
      RoleId = roleId,
      MenuId = m.MenuId,
      CanRead = m.CanRead,
      CanCreate = m.CanCreate,
      CanUpdate = m.CanUpdate,
      CanDelete = m.CanDelete,
    });

    await _menuRoleRepo.InsertRange(entities);
  }
}
