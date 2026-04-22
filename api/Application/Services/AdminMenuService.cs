using Drink.Application.Interfaces;
using Drink.Application.Mappings;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Drink.Application.Services;

public class AdminMenuService : BaseService
{
  private readonly IGenericRepository<AdminUser> _userRepo;
  private readonly IGenericRepository<AdminMenuRole> _menuRoleRepo;
  private readonly IGenericRepository<AdminMenu> _menuRepo;
  private readonly IGenericRepository<AdminRole> _roleRepo;

  public AdminMenuService(
    ICurrentUserContext currentUser,
    IGenericRepository<AdminUser> userRepo,
    IGenericRepository<AdminMenuRole> menuRoleRepo,
    IGenericRepository<AdminMenu> menuRepo,
    IGenericRepository<AdminRole> roleRepo) : base(currentUser)
  {
    _userRepo = userRepo;
    _menuRoleRepo = menuRoleRepo;
    _menuRepo = menuRepo;
    _roleRepo = roleRepo;
  }

  public async Task<ApiResponse> GetMyMenus()
  {
    var user = await _userRepo.Get(u => u.Id == CurrentUserId);
    if (user is null)
      return Fail(Constants.ErrorCodes.Unauthorized, "使用者不存在");

    // 判斷是否為系統角色
    var role = await _roleRepo.GetById(user.RoleId);
    var isSystemRole = role?.IsSystem ?? false;

    // 取得該 Role 的所有 MenuRole 權限
    var menuRoles = await _menuRoleRepo.GetList(
      predicate: mr => mr.RoleId == user.RoleId);
    var menuRoleDict = menuRoles.ToDictionary(mr => mr.MenuId);

    // 取得可存取的 MenuId（系統角色全部可存取）
    List<int> accessibleMenuIds;
    if (isSystemRole)
    {
      accessibleMenuIds = await _menuRepo.Query
        .Where(m => m.Endpoint != null)
        .Select(m => m.Id)
        .ToListAsync();
    }
    else
    {
      accessibleMenuIds = menuRoles
        .Where(mr => mr.CanRead || mr.CanCreate || mr.CanUpdate || mr.CanDelete)
        .Select(mr => mr.MenuId)
        .ToList();
    }

    // 取得所有 Menu
    var allMenus = await _menuRepo.GetList(
      order: q => q.OrderBy(m => m.Sort));

    // 建立樹狀結構，僅包含可存取的葉節點及其父節點
    var tree = BuildMenuTree(allMenus, accessibleMenuIds, menuRoleDict, isSystemRole, parentId: null);

    return Success((object)tree);
  }

  private List<MenuTreeResponse> BuildMenuTree(
    List<AdminMenu> allMenus,
    List<int> accessibleMenuIds,
    Dictionary<int, AdminMenuRole> menuRoleDict,
    bool isSystemRole,
    int? parentId)
  {
    var result = new List<MenuTreeResponse>();

    var children = allMenus.Where(m => m.ParentId == parentId).ToList();

    foreach (var menu in children)
    {
      var subTree = BuildMenuTree(allMenus, accessibleMenuIds, menuRoleDict, isSystemRole, menu.Id);

      // 葉節點：必須在可存取清單中
      // 群組節點：至少有一個可存取的子節點
      var isLeaf = menu.Endpoint is not null;
      if (isLeaf && !accessibleMenuIds.Contains(menu.Id))
        continue;
      if (!isLeaf && subTree.Count == 0)
        continue;

      var dto = menu.ToMenuTreeResponse();
      dto.Children = subTree;

      // 填入 CRUD 權限
      if (isSystemRole)
      {
        dto.CanRead = true;
        dto.CanCreate = true;
        dto.CanUpdate = true;
        dto.CanDelete = true;
      }
      else if (menuRoleDict.TryGetValue(menu.Id, out var mr))
      {
        dto.CanRead = mr.CanRead;
        dto.CanCreate = mr.CanCreate;
        dto.CanUpdate = mr.CanUpdate;
        dto.CanDelete = mr.CanDelete;
      }

      result.Add(dto);
    }

    return result;
  }
}
