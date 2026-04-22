using Drink.Application.Attributes;
using Drink.Application.Constants;
using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drink.Admin.API.Controllers;

[Authorize]
public class RolesController : BaseController
{
  private readonly AdminRoleService _roleService;

  public RolesController(AdminRoleService roleService)
  {
    _roleService = roleService;
  }

  /// <summary>
  /// 取得所有角色
  /// </summary>
  [HttpGet]
  [RequireRole(MenuConstants.AdminRole, CrudAction.Read)]
  [ProducesResponseType(typeof(ApiResponse<List<AdminRoleListResponse>>), 200)]
  public async Task<IActionResult> GetList()
  {
    var result = await _roleService.GetList();
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 取得單一角色（含 Menu CRUD 設定）
  /// </summary>
  [HttpGet("{roleId}")]
  [RequireRole(MenuConstants.AdminRole, CrudAction.Read)]
  [ProducesResponseType(typeof(ApiResponse<AdminRoleDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> GetById(int roleId)
  {
    var result = await _roleService.GetById(roleId);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 建立角色
  /// </summary>
  [HttpPost]
  [RequireRole(MenuConstants.AdminRole, CrudAction.Create)]
  [ProducesResponseType(typeof(ApiResponse<AdminRoleDetailResponse>), 201)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> Create([FromBody] CreateAdminRoleRequest request)
  {
    var result = await _roleService.Create(request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "ROLE_ALREADY_EXISTS" ? 409 : 400;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return StatusCode(201, ApiResponse<AdminRoleDetailResponse>.Success(result.Data));
  }

  /// <summary>
  /// 更新角色（含 Menu CRUD，整批覆蓋）
  /// </summary>
  [HttpPut("{roleId}")]
  [RequireRole(MenuConstants.AdminRole, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse<AdminRoleDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 403)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> Update(int roleId, [FromBody] UpdateAdminRoleRequest request)
  {
    var result = await _roleService.Update(roleId, request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error switch
      {
        "CANNOT_MODIFY_SYSTEM_ROLE" => 403,
        "ROLE_ALREADY_EXISTS" => 409,
        "ROLE_NOT_FOUND" => 404,
        _ => 400
      };
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return ApiOk(result.Data);
  }

  /// <summary>
  /// 刪除角色
  /// </summary>
  [HttpDelete("{roleId}")]
  [RequireRole(MenuConstants.AdminRole, CrudAction.Delete)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 403)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> Delete(int roleId, [FromBody] DeleteAdminRoleRequest? request)
  {
    var result = await _roleService.Delete(roleId, request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error switch
      {
        "CANNOT_DELETE_SYSTEM_ROLE" => 403,
        "ROLE_NOT_FOUND" => 404,
        _ => 400
      };
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus);
    }
    return ApiOk();
  }
}
