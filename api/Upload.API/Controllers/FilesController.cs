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
  /// Upload an image (internal API, requires X-Api-Key).
  /// Pipeline: validate → SkiaSharp decode → resize (max 4000px) → encode webp → SHA-256 dedup.
  /// </summary>
  [HttpPost]
  [ProducesResponseType(typeof(ApiResponse<FileUploadResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 401)]
  public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
  {
    var result = await _uploadService.Upload(file, ct, FileUploadService.DefaultMaxDimension);
    return result.Code == 0 ? Ok(result) : BadRequest(result);
  }

  /// <summary>
  /// Upload an avatar image (internal API, requires X-Api-Key).
  /// Same pipeline as <see cref="Upload"/> but resized to max 512px long edge.
  /// </summary>
  [HttpPost("/api/upload/avatar")]
  [ProducesResponseType(typeof(ApiResponse<FileUploadResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 401)]
  public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken ct)
  {
    var result = await _uploadService.Upload(file, ct, FileUploadService.AvatarMaxDimension);
    return result.Code == 0 ? Ok(result) : BadRequest(result);
  }
}
