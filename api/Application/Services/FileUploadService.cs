using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Responses;
using Drink.Application.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Drink.Application.Services;

public class FileUploadService : BaseService
{
  private readonly IFileStorageService _storageService;
  private readonly UploadSettings _uploadSettings;

  // Extension -> allowed MIME types
  private static readonly Dictionary<string, string[]> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
  {
    [".jpg"]  = ["image/jpeg"],
    [".jpeg"] = ["image/jpeg"],
    [".png"]  = ["image/png"],
    [".gif"]  = ["image/gif"],
    [".webp"] = ["image/webp"],
    [".pdf"]  = ["application/pdf"],
  };

  // Extension -> magic bytes (file signature)
  private static readonly Dictionary<string, byte[][]> FileSignatures = new(StringComparer.OrdinalIgnoreCase)
  {
    [".jpg"]  = [[0xFF, 0xD8, 0xFF]],
    [".jpeg"] = [[0xFF, 0xD8, 0xFF]],
    [".png"]  = [[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]],
    [".gif"]  = [[0x47, 0x49, 0x46, 0x38, 0x37, 0x61], [0x47, 0x49, 0x46, 0x38, 0x39, 0x61]], // GIF87a, GIF89a
    [".webp"] = [[0x52, 0x49, 0x46, 0x46]], // RIFF
    [".pdf"]  = [[0x25, 0x50, 0x44, 0x46]], // %PDF
  };

  public FileUploadService(
    ICurrentUserContext currentUser,
    IFileStorageService storageService,
    IOptions<UploadSettings> uploadSettings) : base(currentUser)
  {
    _storageService = storageService;
    _uploadSettings = uploadSettings.Value;
  }

  public async Task<ApiResponse<FileUploadResponse>> Upload(IFormFile file, string category)
  {
    if (file.Length == 0)
      return Fail<FileUploadResponse>(ErrorCodes.FileUploadFailed, "File is empty.");

    if (file.Length > _uploadSettings.MaxFileSizeBytes)
      return Fail<FileUploadResponse>(ErrorCodes.FileSizeExceeded, $"File size exceeds {_uploadSettings.MaxFileSizeBytes / 1024 / 1024}MB limit.");

    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

    // 1. Extension whitelist
    if (!_uploadSettings.AllowedExtensions.Contains(extension))
      return Fail<FileUploadResponse>(ErrorCodes.FileExtensionNotAllowed, $"Extension '{extension}' is not allowed.");

    // 2. MIME type must match extension
    if (!ValidateMimeType(extension, file.ContentType))
      return Fail<FileUploadResponse>(ErrorCodes.FileExtensionNotAllowed, $"MIME type '{file.ContentType}' does not match extension '{extension}'.");

    // 3. Magic bytes must match extension
    if (!await ValidateFileSignature(extension, file))
      return Fail<FileUploadResponse>(ErrorCodes.FileExtensionNotAllowed, "File content does not match its extension.");

    var result = await _storageService.SaveAsync(file, category);

    return Success(new FileUploadResponse
    {
      FileName = result.OriginalFileName,
      Url = $"/assets/{result.StoredPath}",
      ContentType = result.ContentType,
      FileSize = result.FileSize
    });
  }

  private static bool ValidateMimeType(string extension, string contentType)
  {
    if (!AllowedMimeTypes.TryGetValue(extension, out var allowedTypes))
      return false;

    return allowedTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
  }

  private static async Task<bool> ValidateFileSignature(string extension, IFormFile file)
  {
    if (!FileSignatures.TryGetValue(extension, out var signatures))
      return false;

    var maxLen = signatures.Max(s => s.Length);
    var headerBytes = new byte[maxLen];
    await using var stream = file.OpenReadStream();
    var bytesRead = await stream.ReadAsync(headerBytes.AsMemory(0, maxLen));

    return signatures.Any(signature =>
      bytesRead >= signature.Length &&
      headerBytes.AsSpan(0, signature.Length).SequenceEqual(signature));
  }
}
