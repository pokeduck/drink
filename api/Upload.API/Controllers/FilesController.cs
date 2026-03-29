using Drink.Application.Responses;
using Drink.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Drink.Upload.API.Controllers;

public class FilesController : BaseController
{
  private readonly FileUploadService _uploadService;

  public FilesController(FileUploadService uploadService)
  {
    _uploadService = uploadService;
  }

  /// <summary>
  /// Upload a file (internal API, requires X-Api-Key)
  /// </summary>
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
