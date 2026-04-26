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
[ApiController]
[Route("api/admin/shops/{shopId:int}")]
public class ShopImagesController : BaseController
{
  private readonly AdminShopImageService _service;

  public ShopImagesController(AdminShopImageService service)
  {
    _service = service;
  }

  // ==== List ====

  [HttpGet("images")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Read)]
  [ProducesResponseType(typeof(ApiResponse<ShopImageListResponse>), 200)]
  public async Task<IActionResult> GetList(int shopId, [FromQuery] ShopImageListQuery query)
  {
    var result = await _service.GetList(shopId, query);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk(result.Data);
  }

  [HttpGet("drink-items/{drinkItemId:int}/images")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Read)]
  [ProducesResponseType(typeof(ApiResponse<List<ShopImageResponse>>), 200)]
  public async Task<IActionResult> GetByDrinkItem(int shopId, int drinkItemId)
  {
    var result = await _service.GetByDrinkItem(shopId, drinkItemId);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 404);
    return ApiOk(result.Data);
  }

  // ==== Upload ====

  [HttpPost("images")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Create)]
  [ProducesResponseType(typeof(ApiResponse<ShopImageResponse>), 201)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> Upload(
    int shopId,
    IFormFile file,
    [FromQuery] int? drinkItemId,
    CancellationToken ct)
  {
    var result = await _service.Upload(shopId, file, drinkItemId, ct);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "SHOP_NOT_FOUND" ? 404 : 400;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return StatusCode(201, ApiResponse<ShopImageResponse>.Success(result.Data));
  }

  // ==== Update ====

  [HttpPatch("images/{imageId:int}")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse<ShopImageResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 404)]
  public async Task<IActionResult> Update(int shopId, int imageId, [FromBody] ShopImageUpdateRequest request)
  {
    var result = await _service.Update(shopId, imageId, request);
    if (result.Code != 0)
    {
      var httpStatus = result.Error == "SHOP_IMAGE_NOT_FOUND" ? 404 : 400;
      return ApiError((result.Code, result.Error!), result.Message!, httpStatus, result.Errors);
    }
    return ApiOk(result.Data);
  }

  // ==== Sort batch ====

  [HttpPatch("drink-items/{drinkItemId:int}/images/sort")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Update)]
  [ProducesResponseType(typeof(ApiResponse), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> UpdateSort(int shopId, int drinkItemId, [FromBody] ShopImageSortRequest request)
  {
    var result = await _service.UpdateSort(shopId, drinkItemId, request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 400, result.Errors);
    return ApiOk();
  }

  // ==== Delete ====

  [HttpDelete("images")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Delete)]
  [ProducesResponseType(typeof(ApiResponse<ShopImageBatchDeleteResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  public async Task<IActionResult> BatchDelete(int shopId, [FromBody] ShopImageBatchDeleteRequest request)
  {
    var result = await _service.BatchDelete(shopId, request);
    if (result.Code != 0)
      return ApiError((result.Code, result.Error!), result.Message!, 400, result.Errors);
    return ApiOk(result.Data);
  }

  [HttpDelete("images/orphans")]
  [RequireRole(MenuConstants.ShopList, CrudAction.Delete)]
  [ProducesResponseType(typeof(ApiResponse<ShopImageBatchDeleteResponse>), 200)]
  public async Task<IActionResult> DeleteOrphans(int shopId)
  {
    var result = await _service.DeleteOrphans(shopId);
    return ApiOk(result.Data);
  }
}
