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
  /// 上傳圖片（proxy 至 Upload.API；單檔，多檔由前端佇列序列呼叫）
  /// </summary>
  [HttpPost]
  [ProducesResponseType(typeof(ApiResponse<FileUploadResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 400)]
  [ProducesResponseType(typeof(ApiResponse), 401)]
  public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
  {
    using var client = _httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Add("X-Api-Key", _uploadApiSettings.ApiKey);

    using var content = new MultipartFormDataContent();
    await using var stream = file.OpenReadStream();
    var fileContent = new StreamContent(stream);
    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
    content.Add(fileContent, "file", file.FileName);

    var response = await client.PostAsync(
      $"{_uploadApiSettings.BaseUrl}/api/upload/files",
      content,
      ct);

    var json = await response.Content.ReadAsStringAsync(ct);
    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    if (response.IsSuccessStatusCode)
    {
      var result = JsonSerializer.Deserialize<ApiResponse<FileUploadResponse>>(json, options);
      return Ok(result);
    }

    var error = JsonSerializer.Deserialize<ApiResponse>(json, options);
    return StatusCode((int)response.StatusCode, error);
  }

  /// <summary>
  /// 取得圖片資產的 base URL（前端拼接圖片 URL 用）
  /// </summary>
  [HttpGet("asset-host")]
  [ProducesResponseType(typeof(ApiResponse<AssetHostResponse>), 200)]
  [ProducesResponseType(typeof(ApiResponse), 401)]
  public IActionResult AssetHost()
  {
    return Ok(ApiResponse<AssetHostResponse>.Success(new AssetHostResponse
    {
      BaseUrl = _uploadApiSettings.PublicBaseUrl
    }));
  }
}
