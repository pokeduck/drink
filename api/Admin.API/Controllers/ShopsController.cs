using Drink.Application.Attributes;
using Drink.Application.Constants;
using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Application.Services;
using Drink.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drink.Admin.API.Controllers;

[Authorize]
public class ShopsController : BaseController
{
  private readonly AdminShopService _service;

  public ShopsController(AdminShopService service)
  {
    _service = service;
  }

  // ==================== Shop CRUD ====================

  [HttpGet]
  [RequireRole(MenuConstants.ShopList, CrudAction.Read)]
  [ProducesResponseType(typeof(ApiResponse<PaginationList<ShopListResponse>>), 200)]
  public async Task<IActionResult> GetList(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? sortBy = null,
    [FromQuery] string? sortOrder = null,
    [FromQuery] string? keyword = null,
    [FromQuery] int? status = null)
  {
    var result = await _service.GetList(page, pageSize, sortBy, sortOrder, keyword, status);
    return ApiOk(result.Data);
  }

  [HttpGet("{id}")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Read)]
  [ProducesResponseType(typeof(ApiResponse<ShopDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> GetById(int id)
  {
    var result = await _service.GetById(id);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk(result.Data);
  }

  [HttpPost]
  [RequireRole(MenuConstants.ShopList, CrudAction.Create)]
  [ProducesResponseType(typeof(ApiResponse<ShopDetailResponse>), 201)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> Create([FromBody] CreateShopRequest request)
  {
    var result = await _service.Create(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 409, result.Errors);
    return StatusCode(201, ApiResponse<ShopDetailResponse>.Success(result.Data));
  }

  [HttpPut("{id}")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse<ShopDetailResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> Update(int id, [FromBody] UpdateShopRequest request)
  {
    var result = await _service.Update(id, request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "SHOP_ALREADY_EXISTS" ? 409 : 404;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return ApiOk(result.Data);
  }

  [HttpDelete("{id}")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Delete)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> Delete(int id)
  {
    var result = await _service.Delete(id);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk();
  }

  [HttpPut("sort")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> BatchSort([FromBody] BatchSortRequest request)
  {
    var result = await _service.BatchSort(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 400);
    return ApiOk();
  }

  [HttpDelete("batch")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Delete)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> BatchDelete([FromBody] BatchDeleteRequest request)
  {
    var result = await _service.BatchDelete(request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 400);
    return ApiOk();
  }

  // ==================== Menu Management ====================

  [HttpGet("{shopId}/menu")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Read)]
  [ProducesResponseType(typeof(ApiResponse<AdminShopMenuResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> GetMenu(int shopId)
  {
    var result = await _service.GetMenu(shopId);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk(result.Data);
  }

  [HttpPost("{shopId}/categories")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Create)]
  [ProducesResponseType(typeof(ApiResponse<int>), 201)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> CreateCategory(int shopId, [FromBody] CreateShopCategoryRequest request)
  {
    var result = await _service.CreateCategory(shopId, request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "CATEGORY_ALREADY_EXISTS" ? 409 : 404;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return StatusCode(201, ApiResponse<int>.Success(result.Data));
  }

  [HttpPut("{shopId}/categories/{categoryId}")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  [ProducesResponseType(typeof(ApiResponse), 409)]
  public async Task<IActionResult> UpdateCategory(int shopId, int categoryId, [FromBody] UpdateShopCategoryRequest request)
  {
    var result = await _service.UpdateCategory(shopId, categoryId, request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "CATEGORY_ALREADY_EXISTS" ? 409 : 404;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return ApiOk();
  }

  [HttpDelete("{shopId}/categories/{categoryId}")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Delete)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> DeleteCategory(int shopId, int categoryId)
  {
    var result = await _service.DeleteCategory(shopId, categoryId);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk();
  }

  [HttpPut("{shopId}/categories/sort")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> BatchSortCategories(int shopId, [FromBody] BatchSortRequest request)
  {
    var result = await _service.BatchSortCategories(shopId, request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 400);
    return ApiOk();
  }

  [HttpPost("{shopId}/categories/{categoryId}/items")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Create)]
  [ProducesResponseType(typeof(ApiResponse<int>), 201)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> CreateMenuItem(int shopId, int categoryId, [FromBody] CreateShopMenuItemRequest request)
  {
    var result = await _service.CreateMenuItem(shopId, categoryId, request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error is "CATEGORY_NOT_FOUND" or "SHOP_NOT_FOUND" or "MENU_ITEM_NOT_FOUND" ? 404 : 400;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return StatusCode(201, ApiResponse<int>.Success(result.Data));
  }

  [HttpPut("{shopId}/categories/{categoryId}/items/{itemId}")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> UpdateMenuItem(int shopId, int categoryId, int itemId, [FromBody] UpdateShopMenuItemRequest request)
  {
    var result = await _service.UpdateMenuItem(shopId, categoryId, itemId, request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error is "MENU_ITEM_NOT_FOUND" or "CATEGORY_NOT_FOUND" ? 404 : 400;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return ApiOk();
  }

  [HttpDelete("{shopId}/categories/{categoryId}/items/{itemId}")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Delete)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> DeleteMenuItem(int shopId, int categoryId, int itemId)
  {
    var result = await _service.DeleteMenuItem(shopId, categoryId, itemId);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk();
  }

  [HttpPut("{shopId}/categories/{categoryId}/items/sort")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> BatchSortMenuItems(int shopId, int categoryId, [FromBody] BatchSortRequest request)
  {
    var result = await _service.BatchSortMenuItems(shopId, categoryId, request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 400);
    return ApiOk();
  }

  // ==================== Override ====================

  [HttpGet("{shopId}/overrides")]
  [RequireRole(MenuConstants.ShopOverride, CrudAction.Read)]
  [ProducesResponseType(typeof(ApiResponse<ShopOverrideResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> GetOverrides(int shopId)
  {
    var result = await _service.GetOverrides(shopId);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk(result.Data);
  }

  [HttpPut("{shopId}/overrides")]
  [RequireRole(MenuConstants.ShopOverride, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> UpdateOverrides(int shopId, [FromBody] UpdateShopOverrideRequest request)
  {
    var result = await _service.UpdateOverrides(shopId, request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk();
  }
}
