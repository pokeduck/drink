using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drink.Admin.API.Controllers;

[Authorize]
public class MenusController : BaseController
{
  private readonly AdminMenuService _menuService;

  public MenusController(AdminMenuService menuService)
  {
    _menuService = menuService;
  }

  /// <summary>
  /// 取得當前使用者可存取的 Menu 樹狀結構
  /// </summary>
  [HttpGet("me")]
  [ProducesResponseType(typeof(ApiResponse<List<MenuTreeResponse>>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 401)]
  public async Task<IActionResult> GetMyMenus()
  {
    var result = await _menuService.GetMyMenus();
    return Ok(result);
  }
}
