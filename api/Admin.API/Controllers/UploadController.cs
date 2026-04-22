using System.Text.Json;
using Drink.Application.Responses;
using Drink.Application.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Drink.Admin.API.Controllers;

[Authorize]
public class UploadController : BaseController
{
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly UploadApiSettings _uploadApiSettings;

  public UploadController(IHttpClientFactory httpClientFactory, IOptions<UploadApiSettings> uploadApiSettings)
  {
    _httpClientFactory = httpClientFactory;
    _uploadApiSettings = uploadApiSettings.Value;
  }

  /// <summary>
  /// 上傳檔案（proxy 至 Upload.API）
  /// </summary>
  /// <param name="file">檔案</param>
  /// <param name="category">分類資料夾（如 images, pdf）</param>
  [HttpPost]
  [ProducesResponseType(typeof(ApiResponse<FileUploadResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 401)]
  public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string category = "images")
  {
    using var client = _httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Add("X-Api-Key", _uploadApiSettings.ApiKey);

    using var content = new MultipartFormDataContent();
    await using var stream = file.OpenReadStream();
    var fileContent = new StreamContent(stream);
    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
    content.Add(fileContent, "file", file.FileName);

    var response = await client.PostAsync(
      $"{_uploadApiSettings.BaseUrl}/api/upload/files?category={Uri.EscapeDataString(category)}",
      content);

    var json = await response.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    if (response.IsSuccessStatusCode)
    {
      var result = JsonSerializer.Deserialize<ApiResponse<FileUploadResponse>>(json, options);
      return Ok(result);
    }

    var error = JsonSerializer.Deserialize<ApiResponse>(json, options);
    return StatusCode((int)response.StatusCode, error);
  }
}
