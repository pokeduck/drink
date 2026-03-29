using Drink.Application.Responses;
using Drink.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drink.Admin.API.Controllers;

[Authorize]
public class UploadController : BaseController
{
  private readonly FileUploadService _uploadService;

  public UploadController(FileUploadService uploadService)
  {
    _uploadService = uploadService;
  }

  /// <summary>
  /// 上傳檔案
  /// </summary>
  /// <param name="file">檔案</param>
  /// <param name="category">分類資料夾（如 images, pdf）</param>
  [HttpPost]
  [ProducesResponseType(typeof(ApiResponse<FileUploadResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 401)]
  public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string category = "images")
  {
    var result = await _uploadService.Upload(file, category);
    return result.Code == 0 ? Ok(result) : BadRequest(result);
  }
}
