using Drink.Application.Mappings;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Drink.Application.Services;

public class AdminMenuService : BaseService
{
  public AdminMenuService(IServiceProvider serviceProvider) : base(serviceProvider) { }

  public async Task<ApiResponse> GetMyMenus()
  {
    var userRepo = GetRepository<AdminUser>();
    var menuRoleRepo = GetRepository<AdminMenuRole>();
    var menuRepo = GetRepository<AdminMenu>();

    var user = await userRepo.Get(u => u.Id == CurrentUserId);
    if (user is null)
      return Fail(Constants.ErrorCodes.Unauthorized, "使用者不存在");

    // 取得該 Role 可存取的 MenuId（至少一個 CRUD 為 true）
    var accessibleMenuIds = await menuRoleRepo.Query
      .Where(mr => mr.RoleId == user.RoleId
        && (mr.CanRead || mr.CanCreate || mr.CanUpdate || mr.CanDelete))
      .Select(mr => mr.MenuId)
      .ToListAsync();

    // 取得所有 Menu
    var allMenus = await menuRepo.GetList(
      order: q => q.OrderBy(m => m.Sort));

    // 建立樹狀結構，僅包含可存取的葉節點及其父節點
    var tree = BuildMenuTree(allMenus, accessibleMenuIds, parentId: null);

    return Success((object)tree);
  }

  private List<MenuTreeResponse> BuildMenuTree(
    List<AdminMenu> allMenus,
    List<int> accessibleMenuIds,
    int? parentId)
  {
    var result = new List<MenuTreeResponse>();

    var children = allMenus.Where(m => m.ParentId == parentId).ToList();

    foreach (var menu in children)
    {
      var subTree = BuildMenuTree(allMenus, accessibleMenuIds, menu.Id);

      // 葉節點：必須在可存取清單中
      // 群組節點：至少有一個可存取的子節點
      var isLeaf = menu.Endpoint is not null;
      if (isLeaf && !accessibleMenuIds.Contains(menu.Id))
        continue;
      if (!isLeaf && subTree.Count == 0)
        continue;

      var dto = menu.ToMenuTreeResponse();
      dto.Children = subTree;
      result.Add(dto);
    }

    return result;
  }
}
